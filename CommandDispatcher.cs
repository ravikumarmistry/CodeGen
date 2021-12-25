using Autofac;
using CodeGen.Commands;
using CodeGen.Handlers;
using CommandLine;

namespace CodeGen
{

    public interface ICommandDispatcher
    {
        Task DispatchAsync(params string[] parameters);
    }

    public class CommandDispatcher : ICommandDispatcher
    {
        public static IContainer? Container { get; set; }
        public async Task DispatchAsync(params string[] parameters)
        {
            var result = Parser.Default.ParseArguments<NewCommand, GenCommand>(parameters);
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result),
                                                "Command can not be null.");
            }

            await result.WithParsedAsync<GenCommand>(async (command) =>
            {
                var handler = Container!.ResolveNamed<ICommandHandler>(nameof(GenCommand));
                await handler.ExecuteAsync(command);
            });

            await result.WithParsedAsync<NewCommand>(async (newCommand) =>
            {
                var handler = Container!.ResolveNamed<ICommandHandler>(nameof(NewCommand));
                await handler.ExecuteAsync(newCommand as ICommand);
            });
        }
    }
}