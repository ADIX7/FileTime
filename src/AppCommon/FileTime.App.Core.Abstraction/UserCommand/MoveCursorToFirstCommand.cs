namespace FileTime.App.Core.UserCommand;

public class MoveCursorToFirstCommand : IIdentifiableUserCommand
{
    public const string CommandName = "move_cursor_to_first";
    public static MoveCursorToFirstCommand Instance { get; } = new MoveCursorToFirstCommand();

    private MoveCursorToFirstCommand()
    {
    }

    public string UserCommandID => CommandName;
}