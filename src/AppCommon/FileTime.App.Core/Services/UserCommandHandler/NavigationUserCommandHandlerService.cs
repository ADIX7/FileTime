using FileTime.App.Core.Extensions;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.Core.Services;
using FileTime.Core.Timeline;
using FileTime.Providers.Local;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.App.Core.Services.UserCommandHandler;

public class NavigationUserCommandHandlerService : UserCommandHandlerServiceBase
{
    private readonly IAppState _appState;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILocalContentProvider _localContentProvider;
    private readonly IUserCommandHandlerService _userCommandHandlerService;
    private readonly ITimelessContentProvider _timelessContentProvider;
    private ITabViewModel? _selectedTab;
    private IContainer? _currentLocation;
    private IItemViewModel? _currentSelectedItem;
    private IEnumerable<IItemViewModel> _currentItems = Enumerable.Empty<IItemViewModel>();
    private ViewMode _viewMode;

    public NavigationUserCommandHandlerService(
        IAppState appState,
        IServiceProvider serviceProvider,
        ILocalContentProvider localContentProvider,
        IUserCommandHandlerService userCommandHandlerService,
        ITimelessContentProvider timelessContentProvider) : base(appState)
    {
        _appState = appState;
        _serviceProvider = serviceProvider;
        _localContentProvider = localContentProvider;
        _userCommandHandlerService = userCommandHandlerService;
        _timelessContentProvider = timelessContentProvider;

        SaveSelectedTab(t => _selectedTab = t);
        SaveCurrentSelectedItem(i => _currentSelectedItem = i);
        SaveCurrentLocation(l => _currentLocation = l);
        SaveCurrentItems(i => _currentItems = i);

        appState.ViewMode.Subscribe(v => _viewMode = v);

        AddCommandHandlers(new IUserCommandHandler[]
        {
            new TypeUserCommandHandler<CloseTabCommand>(CloseTab),
            new TypeUserCommandHandler<EnterRapidTravelCommand>(EnterRapidTravel),
            new TypeUserCommandHandler<ExitRapidTravelCommand>(ExitRapidTravel),
            new TypeUserCommandHandler<GoUpCommand>(GoUp),
            new TypeUserCommandHandler<MoveCursorDownCommand>(MoveCursorDown),
            new TypeUserCommandHandler<MoveCursorUpCommand>(MoveCursorUp),
            new TypeUserCommandHandler<OpenContainerCommand>(OpenContainer),
            new TypeUserCommandHandler<OpenSelectedCommand>(OpenSelected),
            new TypeUserCommandHandler<SwitchToTabCommand>(SwitchToTab),
        });
    }

    private async Task OpenContainer(OpenContainerCommand command)
    {
        var resolvedPath = await command.Path.ResolveAsync();
        if (resolvedPath is not IContainer resolvedContainer) return;

        _selectedTab?.Tab?.SetCurrentLocation(resolvedContainer);
    }

    private Task OpenSelected()
    {
        if (_currentSelectedItem is not IContainerViewModel containerViewModel || containerViewModel.Container is null)
            return Task.CompletedTask;

        _selectedTab?.Tab?.SetCurrentLocation(containerViewModel.Container);
        return Task.CompletedTask;
    }

    private async Task GoUp()
    {
        if (_currentLocation?.Parent is not AbsolutePath parentPath ||
            await parentPath.ResolveAsyncSafe() is not IContainer newContainer) return;
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

        _selectedTab.Tab?.SetSelectedItem(newSelectedItem.ToAbsolutePath(_timelessContentProvider));
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

    private Task SwitchToTab(SwitchToTabCommand command)
    {
        var number = command.TabNumber;
        var tabViewModel = _appState.Tabs.FirstOrDefault(t => t.TabNumber == number);

        if (number == -1)
        {
            var greatestNumber = _appState.Tabs.Max(t => t.TabNumber);
            tabViewModel = _appState.Tabs.FirstOrDefault(t => t.TabNumber == greatestNumber);
        }
        else if (tabViewModel == null)
        {
            var tab = _serviceProvider.GetInitableResolver<IContainer>(_currentLocation ?? _localContentProvider)
                .GetRequiredService<ITab>();
            var newTabViewModel = _serviceProvider.GetInitableResolver(tab, number).GetRequiredService<ITabViewModel>();

            _appState.AddTab(newTabViewModel);
            tabViewModel = newTabViewModel;
        }

        if (_viewMode == ViewMode.RapidTravel)
        {
            _userCommandHandlerService.HandleCommandAsync(ExitRapidTravelCommand.Instance);
        }

        _appState.SetSelectedTab(tabViewModel!);

        return Task.CompletedTask;
    }

    private Task CloseTab()
    {
        if (_appState.Tabs.Count < 2 || _selectedTab == null) return Task.CompletedTask;

        var tabToRemove = _selectedTab;
        _appState.RemoveTab(tabToRemove!);

        try
        {
            tabToRemove.Dispose();
        }
        catch
        {
        }

        return Task.CompletedTask;
    }
}