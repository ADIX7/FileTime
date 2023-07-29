namespace FileTime.App.Core.UserCommand;

public sealed class RunOrOpenCommand : IIdentifiableUserCommand
{
    public const string CommandName = "run_or_open";
    public static RunOrOpenCommand Instance { get; } = new();

    private RunOrOpenCommand()
    {
    }

    public string UserCommandID => CommandName;

    public string Title => "Open or run";
}