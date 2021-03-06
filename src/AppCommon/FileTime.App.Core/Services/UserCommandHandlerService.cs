using Microsoft.Extensions.DependencyInjection;

namespace FileTime.App.Core.Services;

public class UserCommandHandlerService : IUserCommandHandlerService
{
    private readonly Lazy<IEnumerable<IUserCommandHandler>> _commandHandlers;

    public UserCommandHandlerService(IServiceProvider serviceProvider)
    {
        _commandHandlers = new Lazy<IEnumerable<IUserCommandHandler>>(serviceProvider.GetServices<IUserCommandHandler>);

        //(Commands.AutoRefresh, ToggleAutoRefresh),
        //(Commands.ChangeTimelineMode, ChangeTimelineMode),
        //(Commands.CloseTab, CloseTab),
        //(Commands.Compress, Compress),
        //(Commands.Copy, Copy),
        //(Commands.CopyHash, CopyHash),
        //(Commands.CopyPath, CopyPath),
        //(Commands.CreateContainer, CreateContainer),
        //(Commands.CreateElement, CreateElement),
        //(Commands.Cut, Cut),
        //(Commands.Edit, Edit),
        //(Commands.EnterRapidTravel, EnterRapidTravelMode),
        //(Commands.FindByName, FindByName),
        //(Commands.FindByNameRegex, FindByNameRegex),
        //(Commands.GoToHome, GotToHome),
        //(Commands.GoToPath, GoToContainer),
        //(Commands.GoToProvider, GotToProvider),
        //(Commands.GoToRoot, GotToRoot),
        //(Commands.HardDelete, HardDelete),
        //(Commands.Mark, MarkCurrentItem),
        //(Commands.MoveCursorDownPage, MoveCursorDownPage),
        //(Commands.MoveCursorUpPage, MoveCursorUpPage),
        //(Commands.MoveToFirst, MoveToFirst),
        //(Commands.MoveToLast, MoveToLast),
        //(Commands.NextTimelineBlock, SelectNextTimelineBlock),
        //(Commands.NextTimelineCommand, SelectNextTimelineCommand),
        //(Commands.OpenInFileBrowser, OpenInDefaultFileExplorer),
        //(Commands.OpenOrRun, OpenOrRun),
        //(Commands.PasteMerge, PasteMerge),
        //(Commands.PasteOverwrite, PasteOverwrite),
        //(Commands.PasteSkip, PasteSkip),
        //(Commands.PinFavorite, PinFavorite),
        //(Commands.PreviousTimelineBlock, SelectPreviousTimelineBlock),
        //(Commands.PreviousTimelineCommand, SelectPreviousTimelineCommand),
        //(Commands.Refresh, RefreshCurrentLocation),
        //(Commands.Rename, Rename),
        //(Commands.RunCommand, RunCommandInContainer),
        //(Commands.ScanContainerSize, ScanContainerSize),
        //(Commands.ShowAllShotcut, ShowAllShortcut),
        //(Commands.SoftDelete, SoftDelete),
        //(Commands.SwitchToLastTab, async() => await SwitchToTab(-1)),
        //(Commands.SwitchToTab1, async() => await SwitchToTab(1)),
        //(Commands.SwitchToTab2, async() => await SwitchToTab(2)),
        //(Commands.SwitchToTab3, async() => await SwitchToTab(3)),
        //(Commands.SwitchToTab4, async() => await SwitchToTab(4)),
        //(Commands.SwitchToTab5, async() => await SwitchToTab(5)),
        //(Commands.SwitchToTab6, async() => await SwitchToTab(6)),
        //(Commands.SwitchToTab7, async() => await SwitchToTab(7)),
        //(Commands.SwitchToTab8, async() => await SwitchToTab(8)),
        //(Commands.TimelinePause, PauseTimeline),
        //(Commands.TimelineRefresh, RefreshTimeline),
        //(Commands.TimelineStart, ContinueTimeline),
        //(Commands.ToggleAdvancedIcons, ToggleAdvancedIcons),
        //(Commands.ToggleHidden, ToggleHidden),
    }

    public async Task HandleCommandAsync(UserCommand.IUserCommand command)
    {
        var handler = _commandHandlers.Value.FirstOrDefault(h => h.CanHandleCommand(command));

        if (handler != null)
        {
            await handler.HandleCommandAsync(command);
        }
    }

    public async Task HandleCommandAsync<TUserCommand>() where TUserCommand : UserCommand.IUserCommand, new()
        => await HandleCommandAsync(new TUserCommand());
}