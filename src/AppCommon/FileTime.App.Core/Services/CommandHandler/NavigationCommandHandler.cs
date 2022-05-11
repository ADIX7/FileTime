using System.Reactive.Linq;
using FileTime.App.Core.Command;
using FileTime.App.Core.Extensions;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.Core.Services;
using FileTime.Providers.Local;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.App.Core.Services.CommandHandler;

public class NavigationCommandHandler : CommandHandlerBase
{
    private readonly IAppState _appState;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILocalContentProvider _localContentProvider;
    private readonly ICommandHandlerService _commandHandlerService;
    private ITabViewModel? _selectedTab;
    private IContainer? _currentLocation;
    private IItemViewModel? _currentSelectedItem;
    private IEnumerable<IItemViewModel> _currentItems = Enumerable.Empty<IItemViewModel>();
    private ViewMode _viewMode;

    public NavigationCommandHandler(
        IAppState appState,
        IServiceProvider serviceProvider,
        ILocalContentProvider localContentProvider,
        ICommandHandlerService commandHandlerService) : base(appState)
    {
        _appState = appState;
        _serviceProvider = serviceProvider;
        _localContentProvider = localContentProvider;
        _commandHandlerService = commandHandlerService;

        SaveSelectedTab(t => _selectedTab = t);
        SaveCurrentSelectedItem(i => _currentSelectedItem = i);
        SaveCurrentLocation(l => _currentLocation = l);
        SaveCurrentItems(i => _currentItems = i);

        appState.ViewMode.Subscribe(v => _viewMode = v);

        AddCommandHandlers(new (Command.Command, Func<Task>)[]
        {
            (Command.Command.CloseTab, CloseTab),
            (Command.Command.EnterRapidTravel, EnterRapidTravel),
            (Command.Command.ExitRapidTravel, ExitRapidTravel),
            (Command.Command.GoUp, GoUp),
            (Command.Command.MoveCursorDown, MoveCursorDown),
            (Command.Command.MoveCursorUp, MoveCursorUp),
            (Command.Command.Open, OpenContainer),
            (Command.Command.SwitchToLastTab, async () => await SwitchToTab(-1)),
            (Command.Command.SwitchToTab1, async () => await SwitchToTab(1)),
            (Command.Command.SwitchToTab2, async () => await SwitchToTab(2)),
            (Command.Command.SwitchToTab3, async () => await SwitchToTab(3)),
            (Command.Command.SwitchToTab4, async () => await SwitchToTab(4)),
            (Command.Command.SwitchToTab5, async () => await SwitchToTab(5)),
            (Command.Command.SwitchToTab6, async () => await SwitchToTab(6)),
            (Command.Command.SwitchToTab7, async () => await SwitchToTab(7)),
            (Command.Command.SwitchToTab8, async () => await SwitchToTab(8)),
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

    private Task ExitRapidTravel()
    {
        _appState.SwitchViewMode(ViewMode.Default);
        return Task.CompletedTask;
    }

    private Task SwitchToTab(int number)
    {
        var tabViewModel = _appState.Tabs.FirstOrDefault(t => t.TabNumber == number);

        if (number == -1)
        {
            var greatestNumber = _appState.Tabs.Max(t => t.TabNumber);
            tabViewModel = _appState.Tabs.FirstOrDefault(t => t.TabNumber == greatestNumber);
        }
        else if (tabViewModel == null)
        {
            var tab = _serviceProvider.GetInitableResolver<IContainer>(_currentLocation ?? _localContentProvider).GetRequiredService<ITab>();
            var newTabViewModel = _serviceProvider.GetInitableResolver(tab, number).GetRequiredService<ITabViewModel>();

            _appState.AddTab(newTabViewModel);
            tabViewModel = newTabViewModel;
        }

        if (_viewMode == ViewMode.RapidTravel)
        {
            _commandHandlerService.HandleCommandAsync(Command.Command.ExitRapidTravel);
        }

        _appState.SetSelectedTab(tabViewModel!);

        return Task.CompletedTask;
    }

    private Task CloseTab()
    {
        if (_appState.Tabs.Count < 2) return Task.CompletedTask;
        
        _appState.RemoveTab(_selectedTab!);
        
        return Task.CompletedTask;
    }
}