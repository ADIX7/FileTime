using FileTime.App.Core.Models.Enums;

namespace FileTime.App.Core.UserCommand;

public sealed class PasteFilesFromClipboardCommand : IIdentifiableUserCommand
{
    public const string PasteMergeCommandName = "paste_clipboard_merge";
    public const string PasteOverwriteCommandName = "paste_clipboard_overwrite";
    public const string PasteSkipCommandName = "paste_clipboard_skip";

    public static readonly PasteFilesFromClipboardCommand Merge
        = new(PasteMode.Merge, PasteMergeCommandName, "Paste from clipboard (merge)");
    public static readonly PasteFilesFromClipboardCommand Overwrite
        = new(PasteMode.Overwrite, PasteOverwriteCommandName, "Paste from clipboard (overwrite)");
    public static readonly PasteFilesFromClipboardCommand Skip
        = new(PasteMode.Skip, PasteSkipCommandName, "Paste from clipboard (skip)");
    public PasteMode PasteMode { get; }

    private PasteFilesFromClipboardCommand(PasteMode pasteMode, string commandName, string title)
    {
        PasteMode = pasteMode;
        UserCommandID = commandName;
        Title = title;
    }

    public string UserCommandID { get; }

    public string Title { get; }
}