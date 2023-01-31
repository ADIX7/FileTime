namespace FileTime.App.Core.UserCommand;

public sealed class StartCommandSchedulerCommand : IIdentifiableUserCommand
{
    public const string CommandName = "start_command_scheduler";
    public static StartCommandSchedulerCommand Instance { get; } = new();

    private StartCommandSchedulerCommand()
    {
    }

    public string UserCommandID => CommandName;
}