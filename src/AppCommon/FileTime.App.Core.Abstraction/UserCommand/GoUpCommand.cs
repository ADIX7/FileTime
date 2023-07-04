namespace FileTime.App.Core.UserCommand;

public sealed class GoUpCommand : IIdentifiableUserCommand
{
    public const string CommandName = "go_up";
    public static GoUpCommand Instance { get; } = new();

    private GoUpCommand()
    {
    }

    public string UserCommandID => CommandName;
}