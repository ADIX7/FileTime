namespace FileTime.App.Core.UserCommand;

public sealed class CopyBase64Command : IIdentifiableUserCommand
{
    public const string CommandName = "copy_base64";
    public static CopyBase64Command Instance { get; } = new();

    private CopyBase64Command()
    {
    }

    public string UserCommandID => CommandName;

    public string Title => "Copy content as base64 text";
}