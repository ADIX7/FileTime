namespace FileTime.App.Core.UserCommand;

public class CopyFilesToClipboardCommand : IIdentifiableUserCommand
{
    public const string CommandName = "copy_to_clipboard";

    public static readonly CopyFilesToClipboardCommand Instance = new();

    private CopyFilesToClipboardCommand()
    {
    }

    public string UserCommandID => CommandName;
}