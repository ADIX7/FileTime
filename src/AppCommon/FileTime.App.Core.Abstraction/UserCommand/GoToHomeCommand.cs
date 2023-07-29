namespace FileTime.App.Core.UserCommand;

public sealed class GoToHomeCommand : IIdentifiableUserCommand
{
    public const string CommandName = "go_to_home";
    public static GoToHomeCommand Instance { get; } = new();

    private GoToHomeCommand()
    {
    }

    public string UserCommandID => CommandName;

    public string Title => "Go home";
}