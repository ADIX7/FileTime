namespace FileTime.App.Core.UserCommand;

public class GoForwardCommand : IIdentifiableUserCommand
{
    public const string CommandName = "go_forward";

    public static GoForwardCommand Instance { get; } = new();

    private GoForwardCommand()
    {
    }

    public string UserCommandID => CommandName;
    public string Title => "Go forward";
}