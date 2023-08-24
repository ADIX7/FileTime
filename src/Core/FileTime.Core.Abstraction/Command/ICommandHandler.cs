namespace FileTime.Core.Command;

public interface ICommandHandler
{
    Task<bool> CanHandleAsync(ICommand command);
    Task ExecuteAsync(ICommand command);
}