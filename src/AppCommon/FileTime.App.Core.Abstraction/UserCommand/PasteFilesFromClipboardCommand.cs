using FileTime.App.Core.Models.Enums;

namespace FileTime.App.Core.UserCommand;

public class PasteFilesFromClipboardCommand : IIdentifiableUserCommand
{
    public const string PasteMergeCommandName = "paste_clipboard_merge";
    public const string PasteOverwriteCommandName = "paste_clipboard_overwrite";
    public const string PasteSkipCommandName = "paste_clipboard_skip";
    
    public static readonly PasteFilesFromClipboardCommand Merge = new(PasteMode.Merge, PasteMergeCommandName);
    public static readonly PasteFilesFromClipboardCommand Overwrite = new(PasteMode.Overwrite, PasteOverwriteCommandName);
    public static readonly PasteFilesFromClipboardCommand Skip = new(PasteMode.Skip, PasteSkipCommandName);
    public PasteMode PasteMode { get; }

    private PasteFilesFromClipboardCommand(PasteMode pasteMode, string commandName)
    {
        PasteMode = pasteMode;
        UserCommandID = commandName;
    }

    public string UserCommandID { get; }
}