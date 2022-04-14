using System.Reactive.Linq;
using FileTime.App.Core.Command;
using FileTime.App.Core.Extensions;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;

namespace FileTime.App.Core.Services
{
    public class CommandHandlerService : ICommandHandlerService
    {
        private readonly Dictionary<Commands, Func<Task>> _commandHandlers;
        private readonly IAppState _appState;
        private ITabViewModel? _selectedTab;
        private IContainer? _currentLocation;
        private IItemViewModel? _currentSelectedItem;
        private List<IItemViewModel> _currentItems = new();

        public CommandHandlerService(IAppState appState)
        {
            _appState = appState;

            _appState.SelectedTab.Subscribe(t => _selectedTab = t);
            _appState.SelectedTab.Select(t => t == null ? Observable.Return<IContainer?>(null) : t.CurrentLocation).Switch().Subscribe(l => _currentLocation = l);
            _appState.SelectedTab.Select(t => t == null ? Observable.Return<IItemViewModel?>(null) : t.CurrentSelectedItem).Switch().Subscribe(l => _currentSelectedItem = l);
            _appState.SelectedTab.Select(t => t == null ? Observable.Return(Enumerable.Empty<IItemViewModel>()) : t.CurrentItems).Switch().Subscribe(i => _currentItems = i.ToList());

            _commandHandlers = new Dictionary<Commands, Func<Task>>
            {
                //{Commands.AutoRefresh, ToggleAutoRefresh},
                //{Commands.ChangeTimelineMode, ChangeTimelineMode},
                //{Commands.CloseTab, CloseTab},
                //{Commands.Compress, Compress},
                //{Commands.Copy, Copy},
                //{Commands.CopyHash, CopyHash},
                //{Commands.CopyPath, CopyPath},
                //{Commands.CreateContainer, CreateContainer},
                //{Commands.CreateElement, CreateElement},
                //{Commands.Cut, Cut},
                //{Commands.Edit, Edit},
                //{Commands.EnterRapidTravel, EnterRapidTravelMode},
                //{Commands.FindByName, FindByName},
                //{Commands.FindByNameRegex, FindByNameRegex},
                //{Commands.GoToHome, GotToHome},
                //{Commands.GoToPath, GoToContainer},
                //{Commands.GoToProvider, GotToProvider},
                //{Commands.GoToRoot, GotToRoot},
                {Commands.GoUp, GoUp},
                //{Commands.HardDelete, HardDelete},
                //{Commands.Mark, MarkCurrentItem},
                {Commands.MoveCursorDown, MoveCursorDown},
                //{Commands.MoveCursorDownPage, MoveCursorDownPage},
                {Commands.MoveCursorUp, MoveCursorUp},
                //{Commands.MoveCursorUpPage, MoveCursorUpPage},
                //{Commands.MoveToFirst, MoveToFirst},
                //{Commands.MoveToLast, MoveToLast},
                //{Commands.NextTimelineBlock, SelectNextTimelineBlock},
                //{Commands.NextTimelineCommand, SelectNextTimelineCommand},
                {Commands.Open, OpenContainer},
                //{Commands.OpenInFileBrowser, OpenInDefaultFileExplorer},
                //{Commands.OpenOrRun, OpenOrRun},
                //{Commands.PasteMerge, PasteMerge},
                //{Commands.PasteOverwrite, PasteOverwrite},
                //{Commands.PasteSkip, PasteSkip},
                //{Commands.PinFavorite, PinFavorite},
                //{Commands.PreviousTimelineBlock, SelectPreviousTimelineBlock},
                //{Commands.PreviousTimelineCommand, SelectPreviousTimelineCommand},
                //{Commands.Refresh, RefreshCurrentLocation},
                //{Commands.Rename, Rename},
                //{Commands.RunCommand, RunCommandInContainer},
                //{Commands.ScanContainerSize, ScanContainerSize},
                //{Commands.ShowAllShotcut, ShowAllShortcut},
                //{Commands.SoftDelete, SoftDelete},
                //{Commands.SwitchToLastTab, async() => await SwitchToTab(-1)},
                //{Commands.SwitchToTab1, async() => await SwitchToTab(1)},
                //{Commands.SwitchToTab2, async() => await SwitchToTab(2)},
                //{Commands.SwitchToTab3, async() => await SwitchToTab(3)},
                //{Commands.SwitchToTab4, async() => await SwitchToTab(4)},
                //{Commands.SwitchToTab5, async() => await SwitchToTab(5)},
                //{Commands.SwitchToTab6, async() => await SwitchToTab(6)},
                //{Commands.SwitchToTab7, async() => await SwitchToTab(7)},
                //{Commands.SwitchToTab8, async() => await SwitchToTab(8)},
                //{Commands.TimelinePause, PauseTimeline},
                //{Commands.TimelineRefresh, RefreshTimeline},
                //{Commands.TimelineStart, ContinueTimeline},
                //{Commands.ToggleAdvancedIcons, ToggleAdvancedIcons},
                //{Commands.ToggleHidden, ToggleHidden},
            };
        }

        public async Task HandleCommandAsync(Commands command) =>
            await _commandHandlers[command].Invoke();

        private Task OpenContainer()
        {
            if (_currentSelectedItem is not IContainerViewModel containerViewModel || containerViewModel.Container is null) return Task.CompletedTask;

            _selectedTab?.Tab?.SetCurrentLocation(containerViewModel.Container);
            return Task.CompletedTask;
        }

        private async Task GoUp()
        {
            if (_currentLocation?.Parent is not IAbsolutePath parentPath || await parentPath.ResolveAsyncSafe() is not IContainer newContainer) return;
            _selectedTab?.Tab?.SetCurrentLocation(newContainer);
        }

        private Task MoveCursorDown()
        {
            SelectNewSelectedItem(i => i.SkipWhile(i => i != _currentSelectedItem).Skip(1).FirstOrDefault());
            return Task.CompletedTask;
        }

        private Task MoveCursorUp()
        {
            SelectNewSelectedItem(i => i.TakeWhile(i => i != _currentSelectedItem).LastOrDefault());
            return Task.CompletedTask;
        }

        private void SelectNewSelectedItem(Func<IEnumerable<IItemViewModel>, IItemViewModel?> getNewSelected)
        {
            if (_selectedTab is null || _currentLocation is null) return;

            var newSelectedItem = getNewSelected(_currentItems);
            if (newSelectedItem == null) return;

            _selectedTab.Tab?.SetSelectedItem(newSelectedItem.ToAbsolutePath());
        }
    }
}