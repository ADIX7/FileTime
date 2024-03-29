namespace FileTime.App.Core.UserCommand;

public sealed class RefreshCommand : IIdentifiableUserCommand
{
    public const string CommandName = "refresh";
    public static RefreshCommand Instance { get; } = new();

    private RefreshCommand()
    {
    }

    public string UserCommandID => CommandName;

    public string Title => "Refresh";
}