namespace FileTime.App.Core.UserCommand;

public sealed class CloseTabCommand : IIdentifiableUserCommand
{
    public const string CommandName = "close_tab";
    public static CloseTabCommand Instance { get; } = new();

    private CloseTabCommand()
    {
    }

    public string UserCommandID => CommandName;

    public string Title => "Close tab";
}