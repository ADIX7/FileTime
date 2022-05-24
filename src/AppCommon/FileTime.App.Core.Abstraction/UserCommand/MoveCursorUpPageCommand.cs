namespace FileTime.App.Core.UserCommand;

public class MoveCursorUpPageCommand : IIdentifiableUserCommand
{
    public const string CommandName = "move_cursor_up_page";
    public static MoveCursorUpPageCommand Instance { get; } = new MoveCursorUpPageCommand();

    private MoveCursorUpPageCommand()
    {
    }

    public string UserCommandID => CommandName;
}