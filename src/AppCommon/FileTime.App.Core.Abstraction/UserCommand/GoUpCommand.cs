namespace FileTime.App.Core.UserCommand;

public class GoUpCommand : IIdentifiableUserCommand
{
    public const string CommandName = "go_up";
    public static GoUpCommand Instance { get; } = new GoUpCommand();

    private GoUpCommand()
    {
    }

    public string UserCommandID => CommandName;
}