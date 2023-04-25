using FileTime.App.Core.Models.Enums;

namespace FileTime.App.Core.UserCommand;

public class PasteFilesFromClipboardCommand : IIdentifiableUserCommand
{
    public const string PasteMergeCommandName = "paste_clipboard_merge";
    
    public static readonly PasteFilesFromClipboardCommand Merge = new(PasteMode.Merge, PasteMergeCommandName);
    public PasteMode PasteMode { get; }

    private PasteFilesFromClipboardCommand(PasteMode pasteMode, string commandName)
    {
        PasteMode = pasteMode;
        UserCommandID = commandName;
    }

    public string UserCommandID { get; }
}