namespace FileTime.App.Core.UserCommand;

public class PauseCommandSchedulerCommand : IIdentifiableUserCommand
{
    public const string CommandName = "pause_command_scheduler";
    public static PauseCommandSchedulerCommand Instance { get; } = new();

    private PauseCommandSchedulerCommand()
    {
    }

    public string UserCommandID => CommandName;
}