using CodeGen.Commands;

namespace CodeGen.Handlers
{
    public interface ICommandHandler{
        Task ExecuteAsync(ICommand command);
    }
}