namespace FileTime.App.Core.UserCommand;

public sealed class MoveCursorDownCommand : IIdentifiableUserCommand
{
    public const string CommandName = "move_cursor_down";
    public static MoveCursorDownCommand Instance { get; } = new MoveCursorDownCommand();

    private MoveCursorDownCommand()
    {
    }

    public string UserCommandID => CommandName;
}