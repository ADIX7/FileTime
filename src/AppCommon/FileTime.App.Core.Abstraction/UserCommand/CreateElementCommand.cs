namespace FileTime.App.Core.UserCommand;

public sealed class CreateElementCommand : IIdentifiableUserCommand
{
    public const string CommandName = "create_element";
    public static CreateElementCommand Instance { get; } = new();

    private CreateElementCommand()
    {
    }

    public string UserCommandID => CommandName;
}