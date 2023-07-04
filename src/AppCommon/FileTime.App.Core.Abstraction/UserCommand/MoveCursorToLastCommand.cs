namespace FileTime.App.Core.UserCommand;

public class MoveCursorToLastCommand : IIdentifiableUserCommand
{
    public const string CommandName = "move_cursor_to_last";
    public static MoveCursorToLastCommand Instance { get; } = new();

    private MoveCursorToLastCommand()
    {
    }

    public string UserCommandID => CommandName;
}