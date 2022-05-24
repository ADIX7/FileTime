using FileTime.App.Core.Models.Enums;

namespace FileTime.App.Core.UserCommand;

public sealed class PasteCommand : IIdentifiableUserCommand
{
    public const string PasteMergeCommandName = "paste_merge";
    public const string PasteOverwriteCommandName = "paste_overwrite";
    public const string PasteSkipCommandName = "paste_skip";

    public static PasteCommand Merge { get; } = new PasteCommand(PasteMode.Merge, PasteMergeCommandName);
    public static PasteCommand Overwrite { get; } = new PasteCommand(PasteMode.Overwrite, PasteOverwriteCommandName);
    public static PasteCommand Skip { get; } = new PasteCommand(PasteMode.Skip, PasteSkipCommandName);

    public PasteMode PasteMode { get; }

    private PasteCommand(PasteMode pasteMode, string commandName)
    {
        PasteMode = pasteMode;
        UserCommandID = commandName;
    }

    public string UserCommandID { get; }
}