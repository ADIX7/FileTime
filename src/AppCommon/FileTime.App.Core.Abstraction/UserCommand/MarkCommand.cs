namespace FileTime.App.Core.UserCommand;

public sealed class MarkCommand : IIdentifiableUserCommand
{
    public const string CommandName = "mark_selected";
    public static MarkCommand Instance { get; } = new();

    private MarkCommand()
    {
    }

    public string UserCommandID => CommandName;
}