namespace FileTime.App.Core.UserCommand;

public sealed class MoveCursorToFirstCommand : IIdentifiableUserCommand
{
    public const string CommandName = "move_cursor_to_first";
    public static MoveCursorToFirstCommand Instance { get; } = new();

    private MoveCursorToFirstCommand()
    {
    }

    public string UserCommandID => CommandName;

    public string Title => "Move to first";
}