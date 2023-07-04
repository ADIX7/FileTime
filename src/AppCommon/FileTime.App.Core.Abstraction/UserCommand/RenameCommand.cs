namespace FileTime.App.Core.UserCommand;

public class RenameCommand : IIdentifiableUserCommand
{
    public const string CommandName = "rename";
    public static RenameCommand Instance { get; } = new();

    private RenameCommand()
    {
    }

    public string UserCommandID => CommandName;
}