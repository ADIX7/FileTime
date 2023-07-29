namespace FileTime.App.Core.UserCommand;

public sealed class MoveCursorUpPageCommand : IIdentifiableUserCommand
{
    public const string CommandName = "move_cursor_up_page";
    public static MoveCursorUpPageCommand Instance { get; } = new();

    private MoveCursorUpPageCommand()
    {
    }

    public string UserCommandID => CommandName;

    public string Title => "Move up by page";
}