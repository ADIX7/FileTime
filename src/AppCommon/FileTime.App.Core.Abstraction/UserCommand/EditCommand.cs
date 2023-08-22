namespace FileTime.App.Core.UserCommand;

public class EditCommand : IIdentifiableUserCommand
{
    public const string CommandName = "edit";
    public static readonly EditCommand Instance = new();

    private EditCommand()
    {
    }

    public string UserCommandID => CommandName;
    public string Title => "Edit";
}