namespace FileTime.App.Core.UserCommand;

public sealed class CopyFilesToClipboardCommand : IIdentifiableUserCommand
{
    public const string CommandName = "copy_to_clipboard";

    public static readonly CopyFilesToClipboardCommand Instance = new();

    private CopyFilesToClipboardCommand()
    {
    }

    public string UserCommandID => CommandName;

    public string Title => "Copy to clipbaord";
}