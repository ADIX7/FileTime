namespace FileTime.App.Core.UserCommand;

public sealed class CopyCommand : IIdentifiableUserCommand
{
    public const string CommandName = "copy";
    public static CopyCommand Instance { get; } = new CopyCommand();

    private CopyCommand()
    {
    }

    public string UserCommandID => CommandName;
}