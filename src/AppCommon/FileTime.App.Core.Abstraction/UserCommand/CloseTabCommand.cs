namespace FileTime.App.Core.UserCommand;

public class CloseTabCommand : IIdentifiableUserCommand
{
    public const string CommandName = "close_tab";
    public static CloseTabCommand Instance { get; } = new CloseTabCommand();

    private CloseTabCommand()
    {
    }

    public string UserCommandID => CommandName;
}