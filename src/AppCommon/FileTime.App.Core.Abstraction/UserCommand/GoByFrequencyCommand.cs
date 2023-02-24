namespace FileTime.App.Core.UserCommand;

public class GoByFrequencyCommand : IIdentifiableUserCommand
{
    public const string CommandName = "go_by_frequency";
    
    public static GoByFrequencyCommand Instance { get; } = new();
    
    private GoByFrequencyCommand()
    {
    }

    public string UserCommandID => CommandName;
}