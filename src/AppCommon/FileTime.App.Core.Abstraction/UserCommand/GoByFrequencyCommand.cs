namespace FileTime.App.Core.UserCommand;

public sealed class GoByFrequencyCommand : IIdentifiableUserCommand
{
    public const string CommandName = "go_by_frequency";

    public static GoByFrequencyCommand Instance { get; } = new();

    private GoByFrequencyCommand()
    {
    }

    public string UserCommandID => CommandName;

    public string Title => "Go to frequent place";
}