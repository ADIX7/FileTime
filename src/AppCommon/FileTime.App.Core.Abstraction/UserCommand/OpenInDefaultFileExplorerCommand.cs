namespace FileTime.App.Core.UserCommand;

public sealed class OpenInDefaultFileExplorerCommand : IIdentifiableUserCommand
{
    public const string CommandName = "open_in_default_explorer";
    public static OpenInDefaultFileExplorerCommand Instance { get; } = new();

    private OpenInDefaultFileExplorerCommand()
    {
    }

    public string UserCommandID => CommandName;

    public string Title => "Open in default file browser";
}