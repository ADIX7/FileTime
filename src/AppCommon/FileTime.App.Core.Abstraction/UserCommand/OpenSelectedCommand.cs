namespace FileTime.App.Core.UserCommand;

public sealed class OpenSelectedCommand : IIdentifiableUserCommand
{
    public const string CommandName = "open_selected";
    public static OpenSelectedCommand Instance { get; } = new();

    private OpenSelectedCommand()
    {
    }

    public string UserCommandID => CommandName;
}