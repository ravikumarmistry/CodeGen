using Autofac;
using CodeGen.Commands;
using CodeGen.Exceptions;
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
        private readonly ILifetimeScope scope;

        public CommandDispatcher(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        public async Task DispatchAsync(params string[] parameters)
        {
            var result = Parser.Default.ParseArguments<NewCommand, GenCommand>(parameters);

            result.WithNotParsed((IEnumerable<Error> actions) =>
            {
                // invalid command we cant do any thing.
            });

            await result.WithParsedAsync<GenCommand>(async (command) =>
            {
                var handler = scope!.ResolveNamed<ICommandHandler>(nameof(GenCommand));
                await handler.ExecuteAsync(command);
            });

            await result.WithParsedAsync<NewCommand>(async (newCommand) =>
            {
                var handler = scope!.ResolveNamed<ICommandHandler>(nameof(NewCommand));
                await handler.ExecuteAsync(newCommand as ICommand);
            });
        }
    }
}