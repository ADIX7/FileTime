namespace FileTime.Core.Command;

public interface ICommandHandler
{
    bool CanHandle(ICommand command);
    Task ExecuteAsync(ICommand command);
}