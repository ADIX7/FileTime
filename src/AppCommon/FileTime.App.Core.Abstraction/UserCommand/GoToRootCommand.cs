namespace FileTime.App.Core.UserCommand;

public sealed class GoToRootCommand : IIdentifiableUserCommand
{
    public const string CommandName = "go_to_root";
    public static GoToRootCommand Instance { get; } = new();

    private GoToRootCommand()
    {
    }

    public string UserCommandID => CommandName;

    public string Title => "Go to root";
}