using FileTime.App.Core.UserCommand;

namespace FileTime.ConsoleUI.App.UserCommand;

public class PreviousPreviewUserCommand : IIdentifiableUserCommand
{
    public const string CommandId = "console_previous_preview";

    public static PreviousPreviewUserCommand Instance = new();

    private PreviousPreviewUserCommand()
    {
    }

    public string UserCommandID => CommandId;
    public string Title => "Previous preview";
}