using System.Dynamic;
using System.Reflection;
using CodeGen.Exceptions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using RazorEngineCore;

namespace CodeGen.Utility
{
    public class HookProcessor
    {
        private readonly string templateString;
        private readonly string hookSourceCode;

        private Assembly? assembly;
        private IRazorEngineCompiledTemplate compiledRazorTemplate;

        public HookProcessor(string hookSourceCode)
        {
            this.hookSourceCode = hookSourceCode;
        }

        public void CompileHook()
        {
            // create syntaxtree from source code c#
            var syntaxTree = CSharpSyntaxTree.ParseText(hookSourceCode);

            // create compilation from syntaxtree
            string assemblyName = Path.GetRandomFileName();
            // create refrences from assembly
            var references = new List<MetadataReference>()
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            };

            foreach (var r in ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator))
            {
                references.Add(MetadataReference.CreateFromFile(r));
            }
            // create compilation
            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            // emit assembly to memory
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    string error = "";
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        error = string.Concat(string.Format("{0}: {1}", diagnostic.Id, diagnostic.GetMessage()));
                        // add new line to error
                        error = string.Concat(error, Environment.NewLine);
                    }
                    throw new HookCompileException(error);
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    assembly = Assembly.Load(ms.ToArray());
                }
            }
        }

        public dynamic InvokeHook(string className, string methodName, params object[] args)
        {
            if (assembly == null)
            {
                throw new InvalidOperationException("Assembly is not compiled.");
            }

            var type = assembly.GetType(className);
            var method = type.GetMethod(methodName);
            var instance = Activator.CreateInstance(type);
            dynamic result = method.Invoke(instance, args);
            return result;
        }

        public string ExecuteTemplate(string templateString, dynamic model)
        {
            // comple razor template
            IRazorEngine razorEngine = new RazorEngine();
            compiledRazorTemplate = razorEngine.Compile(templateString);
            string result = compiledRazorTemplate.Run(model);
            return result;
        }
    }
}