namespace FileTime.App.Core.UserCommand;

public sealed class MoveCursorDownPageCommand : IIdentifiableUserCommand
{
    public const string CommandName = "move_cursor_down_page";
    public static MoveCursorDownPageCommand Instance { get; } = new();

    private MoveCursorDownPageCommand()
    {
    }

    public string UserCommandID => CommandName;

    public string Title => "Move down a page";
}