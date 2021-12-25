using System.Text.Json;
using CodeGen.Commands;
using CodeGen.Models;

namespace CodeGen.Handlers
{
    public class NewCommandHandler : ICommandHandler
    {
        public async Task ExecuteAsync(ICommand command)
        {
            var newCommand = command as NewCommand;
            if (newCommand == null)
            {
                throw new ArgumentNullException(nameof(newCommand),
                                                "Command can not be null.");
            }

            // Create a folder in .codegen folder with command name.
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), ".codegen", newCommand.Name);
            // Create a file in the folder with the same name as the command name with extenson .tmpl.json.
            var configFilePath = Path.Combine(folderPath, $"{newCommand.Name}.tmpl.json");
            // Create a file in the folder with the same name as the command name with extenson .defaults.json.
            var defaultsFilePath = Path.Combine(folderPath, $"{newCommand.Name}.settings.json");
            // Create a file in the folder with the same name as the command name with extenson .cshtml.
            var templateFilePath = Path.Combine(folderPath, $"{newCommand.Name}.cshtml");
            // Create a file in the folder with the same name as the command name with extenson .hook.cs.
            var hookFilePath = Path.Combine(folderPath, $"{newCommand.Name}.hook.cs");

            // check if folder exists.
            if (Directory.Exists(folderPath))
            {
                throw new ArgumentException($"Template {folderPath} already exists.");
            }

            // create folder.
            Directory.CreateDirectory(folderPath);

            // create tempalte config
            var tmplConfig = new TemplateConfig
            {
                Name = newCommand.Name,
            };
            File.WriteAllText(configFilePath, JsonSerializer.Serialize(tmplConfig, new JsonSerializerOptions
            {
                WriteIndented = true,
            }));

            // create defaults config
            var defaultsConfig = new DefaultsConfig
            {
                fileExtension = newCommand.FileExtenstion,
                outputDirectory = newCommand.OutputDirectory,
            };
            File.WriteAllText(defaultsFilePath, JsonSerializer.Serialize(defaultsConfig, new JsonSerializerOptions
            {
                WriteIndented = true,
            }));

            // create code file
            File.WriteAllText(templateFilePath, string.Empty);

            //genrate the hook class
            var hookClassString = String.Format(new HookTemplate().template, newCommand.Name);
            File.WriteAllText(hookFilePath, hookClassString);
        }
    }

    public class HookTemplate
    {
        public string template { get; private set; }

        public HookTemplate()
        {
            this.template = 
@"using System;
namespace CodeGenHook
{{
    public class {0}
    {{
        public List<ExpandoObject> BuildTemplateModel(dynamic settings)
        {{
            return new List<ExpandoObject>() {{ new ExpandoObject() }};
        }}
    }}
}}";
        }
    }
}