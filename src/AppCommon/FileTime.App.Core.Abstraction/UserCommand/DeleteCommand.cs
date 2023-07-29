namespace FileTime.App.Core.UserCommand;

public sealed class DeleteCommand : IIdentifiableUserCommand
{
    public const string SoftDeleteCommandName = "soft_delete";
    public const string HardDeleteCommandName = "hard_delete";

    public static DeleteCommand SoftDelete { get; } = new(SoftDeleteCommandName, false, "Delete (soft)");
    public static DeleteCommand HardDelete { get; } = new(HardDeleteCommandName, true, "Delete (hard)");

    private DeleteCommand(string commandName, bool hardDelete, string title)
    {
        UserCommandID = commandName;
        IsHardDelete = hardDelete;
        Title = title;
    }

    public string UserCommandID { get; }
    public bool IsHardDelete { get; }
    public string Title { get; }
}