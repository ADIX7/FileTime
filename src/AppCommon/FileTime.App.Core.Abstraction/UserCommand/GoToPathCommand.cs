namespace FileTime.App.Core.UserCommand;

public sealed class GoToPathCommand : IIdentifiableUserCommand
{
    public const string CommandName = "go_to_path";
    public static GoToPathCommand Instance { get; } = new();

    private GoToPathCommand()
    {
    }

    public string UserCommandID => CommandName;

    public string Title => "Go to path";
}