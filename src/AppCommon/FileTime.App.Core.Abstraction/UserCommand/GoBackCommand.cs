namespace FileTime.App.Core.UserCommand;

public class GoBackCommand : IIdentifiableUserCommand
{
    public const string CommandName = "go_back";

    public static GoBackCommand Instance { get; } = new();

    private GoBackCommand()
    {
    }

    public string UserCommandID => CommandName;
    public string Title => "Go back";
}