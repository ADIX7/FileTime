namespace FileTime.App.Core.UserCommand;

public class GoToPathCommand : IIdentifiableUserCommand
{
    public const string CommandName = "go_to_path";
    public static GoToPathCommand Instance { get; } = new GoToPathCommand();

    private GoToPathCommand()
    {
    }

    public string UserCommandID => CommandName;
}