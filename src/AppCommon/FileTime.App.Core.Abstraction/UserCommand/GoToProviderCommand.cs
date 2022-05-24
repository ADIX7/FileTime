namespace FileTime.App.Core.UserCommand;

public class GoToProviderCommand : IIdentifiableUserCommand
{
    public const string CommandName = "go_to_provider";
    public static GoToProviderCommand Instance { get; } = new GoToProviderCommand();

    private GoToProviderCommand()
    {
    }

    public string UserCommandID => CommandName;
}