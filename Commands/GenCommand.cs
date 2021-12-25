using CommandLine;
using CommandLine.Text;

namespace CodeGen.Commands
{
    [Verb("run", true, HelpText = "Create a new template.")]
    public class GenCommand: ICommand
    {
        [Value(0, MetaName = "template name",
            HelpText = "Template name to be created.",
            Required = true)]
        public string Name { get; set; }

        [Option('e', "extension",
            Default = "",
            HelpText = "Output file extension.")]
        public string FileExtenstion { get; set; }

        [Option('d', "directory",
            Default = ".",
            HelpText = "output directory.")]
        public string OutputDirectory { get; set; }

        [Usage(ApplicationAlias = "codegen")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("normal scenario", new GenCommand { Name = "repository" });
                yield return new Example("specify extension", new GenCommand { Name = "repository", FileExtenstion = "cs" });
                yield return new Example("specify output directory", new GenCommand { Name = "repository", OutputDirectory = "../repositories" });
            }
        }
    }
}