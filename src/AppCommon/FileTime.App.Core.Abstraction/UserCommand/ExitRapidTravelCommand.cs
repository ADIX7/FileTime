namespace FileTime.App.Core.UserCommand;

public class ExitRapidTravelCommand : IIdentifiableUserCommand
{
    public const string CommandName = "exit_rapid_travel_mode";
    public static ExitRapidTravelCommand Instance { get; } = new ExitRapidTravelCommand();

    private ExitRapidTravelCommand()
    {
    }

    public string UserCommandID => CommandName;
}