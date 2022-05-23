using FileTime.Core.Command;

namespace FileTime.Core.Timeline;

public interface ICommandExecutor
{
    void ExecuteCommand(ICommand command);
    event EventHandler<ICommand> CommandFinished;
}