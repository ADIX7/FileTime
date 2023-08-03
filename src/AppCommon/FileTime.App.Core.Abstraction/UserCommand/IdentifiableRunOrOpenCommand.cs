using FileTime.App.Core.ViewModels;

namespace FileTime.App.Core.UserCommand;


public class RunOrOpenCommand : IUserCommand
{
    public IItemViewModel? Item { get; init; }
}

public sealed class IdentifiableRunOrOpenCommand : RunOrOpenCommand, IIdentifiableUserCommand
{
    public const string CommandName = "run_or_open";
    public static IdentifiableRunOrOpenCommand Instance { get; } = new();

    private IdentifiableRunOrOpenCommand()
    {
    }

    public string UserCommandID => CommandName;

    public string Title => "Open or run";
}