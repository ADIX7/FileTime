using FileTime.App.Core.Services;
using FileTime.App.Core.UserCommand;

namespace FileTime.App.Core.StartupServices;

public class DefaultIdentifiableCommandHandlerRegister : IStartupHandler
{
    private readonly IIdentifiableUserCommandService _userCommandHandlerService;

    public DefaultIdentifiableCommandHandlerRegister(IIdentifiableUserCommandService userCommandHandlerService)
    {
        _userCommandHandlerService = userCommandHandlerService;

        AddUserCommand(CloseTabCommand.Instance);
        AddUserCommand(CopyCommand.Instance);
        AddUserCommand(CopyBase64Command.Instance);
        AddUserCommand(CopyFilesToClipboardCommand.Instance);
        AddUserCommand(CopyNativePathCommand.Instance);
        AddUserCommand(CreateContainer.Instance);
        AddUserCommand(CreateElementCommand.Instance);
        AddUserCommand(DeleteCommand.HardDelete);
        AddUserCommand(DeleteCommand.SoftDelete);
        AddUserCommand(EnterRapidTravelCommand.Instance);
        AddUserCommand(ExitRapidTravelCommand.Instance);
        AddUserCommand(GoByFrequencyCommand.Instance);
        AddUserCommand(GoToHomeCommand.Instance);
        AddUserCommand(GoToPathCommand.Instance);
        AddUserCommand(GoToProviderCommand.Instance);
        AddUserCommand(GoToRootCommand.Instance);
        AddUserCommand(GoUpCommand.Instance);
        AddUserCommand(MarkCommand.Instance);
        AddUserCommand(MoveCursorDownCommand.Instance);
        AddUserCommand(MoveCursorDownPageCommand.Instance);
        AddUserCommand(MoveCursorToFirstCommand.Instance);
        AddUserCommand(MoveCursorToLastCommand.Instance);
        AddUserCommand(MoveCursorUpCommand.Instance);
        AddUserCommand(MoveCursorUpPageCommand.Instance);
        AddUserCommand(OpenCommandPaletteCommand.Instance);
        AddUserCommand(OpenInDefaultFileExplorerCommand.Instance);
        AddUserCommand(OpenSelectedCommand.Instance);
        AddUserCommand(PasteCommand.Merge);
        AddUserCommand(PasteCommand.Overwrite);
        AddUserCommand(PasteCommand.Skip);
        AddUserCommand(PasteFilesFromClipboardCommand.Merge);
        AddUserCommand(PasteFilesFromClipboardCommand.Overwrite);
        AddUserCommand(PasteFilesFromClipboardCommand.Skip);
        AddUserCommand(PauseCommandSchedulerCommand.Instance);
        AddUserCommand(RefreshCommand.Instance);
        AddUserCommand(RenameCommand.Instance);
        AddUserCommand(RunOrOpenCommand.Instance);
        AddUserCommand(StartCommandSchedulerCommand.Instance);
        AddUserCommand(SortItemsCommand.OrderByNameCommand);
        AddUserCommand(SortItemsCommand.OrderByNameDescCommand);
        AddUserCommand(SortItemsCommand.OrderByDateCommand);
        AddUserCommand(SortItemsCommand.OrderByDateDescCommand);
        AddUserCommand(IdentifiableSearchCommand.SearchByNameContains);
        AddUserCommand(SwitchToTabCommand.SwitchToLastTab);
        AddUserCommand(SwitchToTabCommand.SwitchToTab1);
        AddUserCommand(SwitchToTabCommand.SwitchToTab2);
        AddUserCommand(SwitchToTabCommand.SwitchToTab3);
        AddUserCommand(SwitchToTabCommand.SwitchToTab4);
        AddUserCommand(SwitchToTabCommand.SwitchToTab5);
        AddUserCommand(SwitchToTabCommand.SwitchToTab6);
        AddUserCommand(SwitchToTabCommand.SwitchToTab7);
        AddUserCommand(SwitchToTabCommand.SwitchToTab8);
    }

    public Task InitAsync() => Task.CompletedTask;

    private void AddUserCommand(IIdentifiableUserCommand command)
        => _userCommandHandlerService.AddIdentifiableUserCommandFactory(command.UserCommandID, command);
}