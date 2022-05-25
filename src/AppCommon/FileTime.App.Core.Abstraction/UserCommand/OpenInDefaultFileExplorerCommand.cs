namespace FileTime.App.Core.UserCommand;

public class OpenInDefaultFileExplorerCommand : IIdentifiableUserCommand
{
    public const string CommandName = "open_in_default_explorer";
    public static OpenInDefaultFileExplorerCommand Instance { get; } = new OpenInDefaultFileExplorerCommand();

    private OpenInDefaultFileExplorerCommand()
    {
    }

    public string UserCommandID => CommandName;
}