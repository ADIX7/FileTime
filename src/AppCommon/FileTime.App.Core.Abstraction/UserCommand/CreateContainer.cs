namespace FileTime.App.Core.UserCommand;

public sealed class CreateContainer : IIdentifiableUserCommand
{
    public const string CommandName = "create_container";
    public static CreateContainer Instance { get; } = new();

    private CreateContainer()
    {
    }

    public string UserCommandID => CommandName;
}