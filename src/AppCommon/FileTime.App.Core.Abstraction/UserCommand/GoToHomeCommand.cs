namespace FileTime.App.Core.UserCommand;

public class GoToHomeCommand : IIdentifiableUserCommand
{
    public const string CommandName = "go_to_home";
    public static GoToHomeCommand Instance { get; } = new GoToHomeCommand();

    private GoToHomeCommand()
    {
    }

    public string UserCommandID => CommandName;
}