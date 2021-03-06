namespace FileTime.App.Core.UserCommand;

public sealed class CreateElement : IIdentifiableUserCommand
{
    public const string CommandName = "create_element";
    public static CreateElement Instance { get; } = new CreateElement();

    private CreateElement()
    {
    }

    public string UserCommandID => CommandName;
}