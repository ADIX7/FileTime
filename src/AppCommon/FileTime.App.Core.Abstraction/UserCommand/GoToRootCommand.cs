namespace FileTime.App.Core.UserCommand;

public class GoToRootCommand : IIdentifiableUserCommand
{
    public const string CommandName = "go_to_root";
    public static GoToRootCommand Instance { get; } = new GoToRootCommand();

    private GoToRootCommand()
    {
    }

    public string UserCommandID => CommandName;
}