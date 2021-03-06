namespace FileTime.App.Core.UserCommand;

public sealed class CreateContainer : IIdentifiableUserCommand
{
    public const string CommandName = "create_container";
    public static CreateContainer Instance { get; } = new CreateContainer();

    private CreateContainer()
    {
    }

    public string UserCommandID => CommandName;
}