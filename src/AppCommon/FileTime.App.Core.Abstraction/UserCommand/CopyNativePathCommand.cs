namespace FileTime.App.Core.UserCommand;

public class CopyNativePathCommand : IIdentifiableUserCommand
{
    public const string CommandName = "copy_path";
    public static CopyNativePathCommand Instance { get; } = new();

    private CopyNativePathCommand()
    {
    }

    public string UserCommandID => CommandName;
}