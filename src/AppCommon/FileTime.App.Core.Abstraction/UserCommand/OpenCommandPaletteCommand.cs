namespace FileTime.App.Core.UserCommand;

public class OpenCommandPaletteCommand : IIdentifiableUserCommand
{
    public const string CommandName = "open_command_palette";
    public static OpenCommandPaletteCommand Instance { get; } = new ();

    private OpenCommandPaletteCommand()
    {
    }

    public string UserCommandID => CommandName;
}