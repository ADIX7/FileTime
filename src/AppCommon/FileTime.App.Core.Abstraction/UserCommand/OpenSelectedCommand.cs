namespace FileTime.App.Core.UserCommand;

public class OpenSelectedCommand : IIdentifiableUserCommand
{
    public const string CommandName = "open_selected";
    public static OpenSelectedCommand Instance { get; } = new OpenSelectedCommand();

    private OpenSelectedCommand()
    {
    }

    public string UserCommandID => CommandName;
}