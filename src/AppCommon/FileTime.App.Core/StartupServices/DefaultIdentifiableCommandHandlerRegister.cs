using FileTime.App.Core.Services;
using FileTime.App.Core.UserCommand;

namespace FileTime.App.Core.StartupServices;

public class DefaultIdentifiableCommandHandlerRegister : IStartupHandler
{
    private readonly IIdentifiableUserCommandService _userCommandHandlerService;

    public DefaultIdentifiableCommandHandlerRegister(IIdentifiableUserCommandService userCommandHandlerService)
    {
        _userCommandHandlerService = userCommandHandlerService;

        AddUserCommand(AddRemoteContentProviderCommand.Instance);
        AddUserCommand(CloseTabCommand.Instance);
        AddUserCommand(CopyBase64Command.Instance);
        AddUserCommand(CopyCommand.Instance);
        AddUserCommand(CopyFilesToClipboardCommand.Instance);
        AddUserCommand(CopyNativePathCommand.Instance);
        AddUserCommand(CreateContainer.Instance);
        AddUserCommand(CreateElementCommand.Instance);
        AddUserCommand(DeleteCommand.HardDelete);
        AddUserCommand(DeleteCommand.SoftDelete);
        AddUserCommand(EditCommand.Instance);
        AddUserCommand(EnterRapidTravelCommand.Instance);
        AddUserCommand(ExitRapidTravelCommand.Instance);
        AddUserCommand(GoBackCommand.Instance);
        AddUserCommand(GoByFrequencyCommand.Instance);
        AddUserCommand(GoForwardCommand.Instance);
        AddUserCommand(GoToHomeCommand.Instance);
        AddUserCommand(GoToPathCommand.Instance);
        AddUserCommand(GoToProviderCommand.Instance);
        AddUserCommand(GoToRootCommand.Instance);
        AddUserCommand(GoUpCommand.Instance);
        AddUserCommand(IdentifiableRunOrOpenCommand.Instance);
        AddUserCommand(IdentifiableSearchCommand.SearchByNameContains);
        AddUserCommand(IdentifiableSearchCommand.SearchByRegex);
        AddUserCommand(MarkCommand.Instance);
        AddUserCommand(MoveCursorDownCommand.Instance);
        AddUserCommand(MoveCursorDownPageCommand.Instance);
        AddUserCommand(MoveCursorToFirstCommand.Instance);
        AddUserCommand(MoveCursorToLastCommand.Instance);
        AddUserCommand(MoveCursorUpCommand.Instance);
        AddUserCommand(MoveCursorUpPageCommand.Instance);
        AddUserCommand(IdentifiableNewTabCommand.Instance);
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
        AddUserCommand(ScanSizeCommand.Instance);
        AddUserCommand(SelectNextTabCommand.Instance);
        AddUserCommand(SelectPreviousTabCommand.Instance);
        AddUserCommand(SortItemsCommand.OrderByCreatedAtCommand);
        AddUserCommand(SortItemsCommand.OrderByCreatedAtDescCommand);
        AddUserCommand(SortItemsCommand.OrderByLastModifiedCommand);
        AddUserCommand(SortItemsCommand.OrderByLastModifiedDescCommand);
        AddUserCommand(SortItemsCommand.OrderByNameCommand);
        AddUserCommand(SortItemsCommand.OrderByNameDescCommand);
        AddUserCommand(SortItemsCommand.OrderBySizeCommand);
        AddUserCommand(SortItemsCommand.OrderBySizeDescCommand);
        AddUserCommand(StartCommandSchedulerCommand.Instance);
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
        => _userCommandHandlerService.AddIdentifiableUserCommand(command);
}