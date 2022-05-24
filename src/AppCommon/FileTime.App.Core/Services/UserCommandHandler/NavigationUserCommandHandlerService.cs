using FileTime.App.Core.Extensions;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.Core.Services;
using FileTime.Core.Timeline;
using FileTime.Providers.Local;
using InitableService;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.App.Core.Services.UserCommandHandler;

public class NavigationUserCommandHandlerService : UserCommandHandlerServiceBase
{
    private const int PageSize = 8;
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
            new TypeUserCommandHandler<GoToHomeCommand>(GoToHome),
            new TypeUserCommandHandler<GoToProviderCommand>(GoToProvider),
            new TypeUserCommandHandler<GoToRootCommand>(GoToRoot),
            new TypeUserCommandHandler<GoUpCommand>(GoUp),
            new TypeUserCommandHandler<MoveCursorDownCommand>(MoveCursorDown),
            new TypeUserCommandHandler<MoveCursorDownPageCommand>(MoveCursorDownPage),
            new TypeUserCommandHandler<MoveCursorToFirstCommand>(MoveCursorToFirst),
            new TypeUserCommandHandler<MoveCursorToLastCommand>(MoveCursorToLast),
            new TypeUserCommandHandler<MoveCursorUpCommand>(MoveCursorUp),
            new TypeUserCommandHandler<MoveCursorUpPageCommand>(MoveCursorUpPage),
            new TypeUserCommandHandler<OpenContainerCommand>(OpenContainer),
            new TypeUserCommandHandler<OpenSelectedCommand>(OpenSelected),
            new TypeUserCommandHandler<RefreshCommand>(Refresh),
            new TypeUserCommandHandler<SwitchToTabCommand>(SwitchToTab),
        });
    }

    private async Task GoToHome()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var resolvedPath =
            await _localContentProvider.GetItemByNativePathAsync(new NativePath(path), PointInTime.Present);
        if (resolvedPath is IContainer homeFolder)
        {
            await _userCommandHandlerService.HandleCommandAsync(
                new OpenContainerCommand(new AbsolutePath(_timelessContentProvider, homeFolder)));
        }
    }

    private async Task GoToRoot()
    {
        var root = _currentLocation;
        if (root is null) return;

        while (true)
        {
            var parent = root.Parent;
            if (parent is null || string.IsNullOrWhiteSpace(parent.Path.Path)) break;
            if (await parent.ResolveAsync() is not IContainer next) break;
            root = next;
        }

        await _userCommandHandlerService.HandleCommandAsync(
            new OpenContainerCommand(new AbsolutePath(_timelessContentProvider, root)));
    }

    private async Task GoToProvider()
    {
        if (_currentLocation is null) return;

        await _userCommandHandlerService.HandleCommandAsync(
            new OpenContainerCommand(new AbsolutePath(_timelessContentProvider, _currentLocation.Provider)));
    }

    private async Task Refresh()
    {
        if (_currentLocation?.FullName is null) return;
        var refreshedItem =
            await _timelessContentProvider.GetItemByFullNameAsync(_currentLocation.FullName, PointInTime.Present);

        if (refreshedItem is not IContainer refreshedContainer) return;

        _selectedTab?.Tab?.ForceSetCurrentLocation(refreshedContainer);
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
        SelectNewSelectedItem(items =>
            items.SkipWhile(i => !i.EqualsTo(_currentSelectedItem)).Skip(1).FirstOrDefault());
        return Task.CompletedTask;
    }

    private Task MoveCursorUp()
    {
        SelectNewSelectedItem(items => items.TakeWhile(i => !i.EqualsTo(_currentSelectedItem)).LastOrDefault());
        return Task.CompletedTask;
    }

    private Task MoveCursorDownPage()
    {
        SelectNewSelectedItem(items =>
        {
            var relevantItems = items.SkipWhile(i => !i.EqualsTo(_currentSelectedItem)).ToList();
            var fallBackItems = relevantItems.Take(PageSize + 1).Reverse();
            var preferredItems = relevantItems.Skip(PageSize + 1);

            return preferredItems.Concat(fallBackItems).FirstOrDefault();
        });
        return Task.CompletedTask;
    }

    private Task MoveCursorUpPage()
    {
        SelectNewSelectedItem(items =>
        {
            var relevantItems = items.TakeWhile(i => !i.EqualsTo(_currentSelectedItem)).Reverse().ToList();
            var fallBackItems = relevantItems.Take(PageSize).Reverse();
            var preferredItems = relevantItems.Skip(PageSize);
            return preferredItems.Concat(fallBackItems).FirstOrDefault();
        });
        return Task.CompletedTask;
    }

    private Task MoveCursorToFirst()
    {
        SelectNewSelectedItem(items => items.FirstOrDefault());
        return Task.CompletedTask;
    }

    private Task MoveCursorToLast()
    {
        SelectNewSelectedItem(items => items.LastOrDefault());
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