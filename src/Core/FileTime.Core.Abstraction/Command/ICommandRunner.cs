using FileTime.Core.Timeline;

namespace FileTime.Core.Command;

public interface ICommandRunner
{
    Task RunCommandAsync(ICommand command);
}