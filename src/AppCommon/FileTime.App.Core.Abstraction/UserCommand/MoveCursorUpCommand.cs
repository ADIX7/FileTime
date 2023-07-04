namespace FileTime.App.Core.UserCommand;

public sealed class MoveCursorUpCommand : IIdentifiableUserCommand
{
    public const string CommandName = "move_cursor_up";
    public static MoveCursorUpCommand Instance { get; } = new();

    private MoveCursorUpCommand()
    {
    }

    public string UserCommandID => CommandName;
}