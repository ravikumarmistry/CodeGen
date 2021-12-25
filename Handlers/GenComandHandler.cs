using System.Dynamic;
using System.Text.Json;
using CodeGen.Commands;
using CodeGen.Models;
using CodeGen.Utility;

namespace CodeGen.Handlers
{
    public class GenCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(ICommand command)
        {
            var genCommand = command as GenCommand;
            if (genCommand == null)
            {
                throw new ArgumentNullException(nameof(genCommand),
                                                "Command can not be null.");
            }

            // check if folder exists with name in .codegen.
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), ".codegen", genCommand.Name);
            // check if cshtml file exists.
            var defaultTemplateFilePath = Path.Combine(folderPath, $"{genCommand.Name}.cshtml");
            // check if hook file exists.
            var hookFilePath = Path.Combine(folderPath, $"{genCommand.Name}.hook.cs");
            // check if config file exists.
            var settingsFilePath = Path.Combine(folderPath, $"{genCommand.Name}.settings.json");

            // verify
            if (!Directory.Exists(folderPath))
            {
                throw new ArgumentException($"Template {folderPath} does not exist.");
            }

            if (!File.Exists(hookFilePath))
            {
                throw new ArgumentException($"Template {hookFilePath} does not exist.");
            }

            if (!File.Exists(settingsFilePath))
            {
                throw new ArgumentException($"Template {settingsFilePath} does not exist.");
            }

            // read settings
            var settings = JsonSerializer.Deserialize<ExpandoObject>(File.ReadAllText(settingsFilePath));

            // read hook
            var hook = File.ReadAllText(hookFilePath);

            // create hookprocessor
            var hookProcessor = new HookProcessor(hook);
            hookProcessor.CompileHook();
            // hook class name
            var hookClassName = "CodeGenHook." + genCommand.Name;
            // invoke hook
            var templateViewModels = hookProcessor.InvokeHook(hookClassName, "BuildTemplateModel", new object[] { settings });


            for (int i = 0; i < templateViewModels.Count; i++)
            {
                // defaults
                var outDirectory = Path.Combine(Directory.GetCurrentDirectory(), genCommand.Name);
                var outFileName = genCommand.Name;
                var templateFilePath = Path.Combine(folderPath, $"{genCommand.Name}.cshtml");

                dynamic templateViewModel = templateViewModels[i];

                if((templateViewModel as IDictionary<string, object>).ContainsKey("OutputDirectory") && !string.IsNullOrWhiteSpace(templateViewModel.OutputDirectory))
                {
                    outDirectory = templateViewModel.OutputDirectory;
                }

                if((templateViewModel as IDictionary<string, object>).ContainsKey("OutputFileName") &&!string.IsNullOrWhiteSpace(templateViewModel.OutputFileName))
                {
                    outFileName = templateViewModel.OutputFileName;
                }
                
                if((templateViewModel as IDictionary<string, object>).ContainsKey("TemplateFilePath") &&!string.IsNullOrWhiteSpace(templateViewModel.TemplateFilePath))
                {
                    templateFilePath = templateViewModel.TemplateFilePath;
                }

                if((templateViewModel as IDictionary<string, object>).ContainsKey("OverrideFile"))
                {
                    // check if out file exists.
                    var outFilePath = Path.Combine(outDirectory, outFileName);
                    if (File.Exists(outFilePath))
                    {
                        // log that file exists on console.
                        Console.WriteLine($"File {outFilePath} already exists. Skipping.");
                        continue;
                    }
                   
                    outDirectory = templateViewModel.OutputDirectory;
                }

                // read template
                var template = File.ReadAllText(templateFilePath);

                // genrate the template file
                var genratedCode = hookProcessor.ExecuteTemplate(template, templateViewModel.Data);


                // create output directory
                if (!Directory.Exists(outDirectory))
                {
                    Directory.CreateDirectory(outDirectory);
                }

                // write file
                File.WriteAllText(Path.Combine(outDirectory, outFileName), genratedCode);
            }

        }

    }
}