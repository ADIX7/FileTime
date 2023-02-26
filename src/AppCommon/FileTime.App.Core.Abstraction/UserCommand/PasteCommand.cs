using FileTime.App.Core.Models.Enums;

namespace FileTime.App.Core.UserCommand;

public sealed class PasteCommand : IIdentifiableUserCommand
{
    public const string PasteMergeCommandName = "paste_merge";
    public const string PasteOverwriteCommandName = "paste_overwrite";
    public const string PasteSkipCommandName = "paste_skip";

    public static readonly PasteCommand Merge = new(PasteMode.Merge, PasteMergeCommandName);
    public static readonly PasteCommand Overwrite = new(PasteMode.Overwrite, PasteOverwriteCommandName);
    public static readonly PasteCommand Skip = new(PasteMode.Skip, PasteSkipCommandName);

    public PasteMode PasteMode { get; }

    private PasteCommand(PasteMode pasteMode, string commandName)
    {
        PasteMode = pasteMode;
        UserCommandID = commandName;
    }

    public string UserCommandID { get; }
}