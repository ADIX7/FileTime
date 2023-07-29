namespace FileTime.App.Core.UserCommand;

public sealed class GoToProviderCommand : IIdentifiableUserCommand
{
    public const string CommandName = "go_to_provider";
    public static GoToProviderCommand Instance { get; } = new();

    private GoToProviderCommand()
    {
    }

    public string UserCommandID => CommandName;

    public string Title => "Go to provider";
}