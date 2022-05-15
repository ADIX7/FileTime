namespace FileTime.App.Core.UserCommand;

public class MarkCommand : IIdentifiableUserCommand
{
    public const string CommandName = "mark_selected";
    public static MarkCommand Instance { get; } = new MarkCommand();

    private MarkCommand()
    {
    }

    public string UserCommandID => CommandName;
}