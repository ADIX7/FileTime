namespace FileTime.App.Core.UserCommand;

public class DeleteCommand : IIdentifiableUserCommand
{
    public const string SoftDeleteCommandName = "soft_delete";
    public const string HardDeleteCommandName = "hard_delete";

    public static DeleteCommand SoftDelete { get; } = new DeleteCommand(SoftDeleteCommandName, false);
    public static DeleteCommand HardDelete { get; } = new DeleteCommand(HardDeleteCommandName, true);

    private DeleteCommand(string commandName, bool hardDelete)
    {
        UserCommandID = commandName;
        IsHardDelete = hardDelete;
    }

    public string UserCommandID { get; }
    public bool IsHardDelete { get; }
}