using System.Reactive.Linq;
using FileTime.App.Core.Command;
using FileTime.App.Core.Extensions;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;

namespace FileTime.App.Core.Services.CommandHandler;

public class NavigationCommandHandler : CommandHandlerBase
{
    private readonly IAppState _appState;
    private ITabViewModel? _selectedTab;
    private IContainer? _currentLocation;
    private IItemViewModel? _currentSelectedItem;
    private IEnumerable<IItemViewModel> _currentItems = Enumerable.Empty<IItemViewModel>();

    public NavigationCommandHandler(IAppState appState) : base(appState)
    {
        _appState = appState;

        SaveSelectedTab(t => _selectedTab = t);
        SaveCurrentSelectedItem(i => _currentSelectedItem = i);
        SaveCurrentLocation(l => _currentLocation = l);
        SaveCurrentItems(i => _currentItems = i);

        AddCommandHandlers(new (Commands, Func<Task>)[]
        {
            (Commands.EnterRapidTravel, EnterRapidTravel),
            (Commands.GoUp, GoUp),
            (Commands.MoveCursorDown, MoveCursorDown),
            (Commands.MoveCursorUp, MoveCursorUp),
            (Commands.Open, OpenContainer),
        });
    }

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
        SelectNewSelectedItem(i => i.SkipWhile(i => !i.EqualsTo(_currentSelectedItem)).Skip(1).FirstOrDefault());
        return Task.CompletedTask;
    }

    private Task MoveCursorUp()
    {
        SelectNewSelectedItem(i => i.TakeWhile(i => !i.EqualsTo(_currentSelectedItem)).LastOrDefault());
        return Task.CompletedTask;
    }

    private void SelectNewSelectedItem(Func<IEnumerable<IItemViewModel>, IItemViewModel?> getNewSelected)
    {
        if (_selectedTab is null || _currentLocation is null) return;

        var newSelectedItem = getNewSelected(_currentItems);
        if (newSelectedItem == null) return;

        _selectedTab.Tab?.SetSelectedItem(newSelectedItem.ToAbsolutePath());
    }

    private Task EnterRapidTravel()
    {
        _appState.SwitchViewMode(ViewMode.RapidTravel);
        return Task.CompletedTask;
    }
}