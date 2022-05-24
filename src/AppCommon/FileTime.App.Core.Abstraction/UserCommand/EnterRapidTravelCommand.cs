namespace FileTime.App.Core.UserCommand;

public sealed class EnterRapidTravelCommand : IIdentifiableUserCommand
{
    public const string CommandName = "exter_rapid_travel_mode";
    public static EnterRapidTravelCommand Instance { get; } = new EnterRapidTravelCommand();

    private EnterRapidTravelCommand()
    {
    }

    public string UserCommandID => CommandName;
}