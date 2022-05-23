using FileTime.Core.Command;

namespace FileTime.Core.Timeline;

public class CommandRunnerContext
{
    public ICommand Command { get; }

    public CommandRunnerContext(ICommand command)
    {
        Command = command;
    }
}