using FileTime.App.Core.UserCommand;

namespace FileTime.ConsoleUI.App.UserCommand;

public class NextPreviewUserCommand : IIdentifiableUserCommand
{
    public const string CommandId = "console_next_preview";

    public static NextPreviewUserCommand Instance = new();

    private NextPreviewUserCommand()
    {
    }

    public string UserCommandID => CommandId;
    public string Title => "Next preview";
}