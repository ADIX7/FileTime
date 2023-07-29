namespace FileTime.App.Core.UserCommand;

public sealed class CopyCommand : IIdentifiableUserCommand
{
    public const string CommandName = "copy";
    public static CopyCommand Instance { get; } = new();

    private CopyCommand()
    {
    }

    public string UserCommandID => CommandName;

    public string Title => "Copy";
}