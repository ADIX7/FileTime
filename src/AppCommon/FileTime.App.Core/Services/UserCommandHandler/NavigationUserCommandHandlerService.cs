using System.Collections.ObjectModel;
using System.Diagnostics;
using DeclarativeProperty;
using FileTime.App.CommandPalette.Services;
using FileTime.App.Core.Configuration;
using FileTime.App.Core.Extensions;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.App.FrequencyNavigation.Services;
using FileTime.Core.ContentAccess;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Services;
using FileTime.Core.Timeline;
using FileTime.Providers.Local;
using InitableService;
using Microsoft.Extensions.Logging;

namespace FileTime.App.Core.Services.UserCommandHandler;

public class NavigationUserCommandHandlerService : UserCommandHandlerServiceBase
{
    private const int PageSize = 8;

    private readonly IAppState _appState;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILocalContentProvider _localContentProvider;
    private readonly IUserCommandHandlerService _userCommandHandlerService;
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly IUserCommunicationService _userCommunicationService;
    private readonly IFrequencyNavigationService _frequencyNavigationService;
    private readonly ICommandPaletteService _commandPaletteService;
    private readonly IContentProviderRegistry _contentProviderRegistry;
    private readonly ILogger<NavigationUserCommandHandlerService> _logger;
    private readonly ApplicationConfiguration _applicationConfiguration;

    private ITabViewModel? _selectedTab;
    private IDeclarativeProperty<IContainer?>? _currentLocation;
    private IDeclarativeProperty<IItemViewModel?>? _currentSelectedItem;
    private IDeclarativeProperty<ObservableCollection<IItemViewModel>?>? _currentItems;
    private ViewMode _viewMode;

    public NavigationUserCommandHandlerService(
        IAppState appState,
        IServiceProvider serviceProvider,
        ILocalContentProvider localContentProvider,
        IUserCommandHandlerService userCommandHandlerService,
        ITimelessContentProvider timelessContentProvider,
        IUserCommunicationService userCommunicationService,
        IFrequencyNavigationService frequencyNavigationService,
        ICommandPaletteService commandPaletteService,
        IContentProviderRegistry contentProviderRegistry,
        ILogger<NavigationUserCommandHandlerService> logger,
        ApplicationConfiguration applicationConfiguration) : base(appState)
    {
        _appState = appState;
        _serviceProvider = serviceProvider;
        _localContentProvider = localContentProvider;
        _userCommandHandlerService = userCommandHandlerService;
        _timelessContentProvider = timelessContentProvider;
        _userCommunicationService = userCommunicationService;
        _frequencyNavigationService = frequencyNavigationService;
        _commandPaletteService = commandPaletteService;
        _contentProviderRegistry = contentProviderRegistry;
        _logger = logger;
        _applicationConfiguration = applicationConfiguration;

        SaveSelectedTab(t => _selectedTab = t);
        SaveCurrentSelectedItem(i => _currentSelectedItem = i);
        SaveCurrentLocation(l => _currentLocation = l);
        SaveCurrentItems(i => _currentItems = i);

        appState.ViewMode.Subscribe(v => _viewMode = v);

        AddCommandHandler(new IUserCommandHandler[]
        {
            new TypeUserCommandHandler<CloseTabCommand>(CloseTab),
            new TypeUserCommandHandler<EnterRapidTravelCommand>(EnterRapidTravel),
            new TypeUserCommandHandler<ExitRapidTravelCommand>(ExitRapidTravel),
            new TypeUserCommandHandler<GoBackCommand>(GoBack),
            new TypeUserCommandHandler<GoByFrequencyCommand>(GoByFrequency),
            new TypeUserCommandHandler<GoForwardCommand>(GoForward),
            new TypeUserCommandHandler<GoToHomeCommand>(GoToHome),
            new TypeUserCommandHandler<GoToPathCommand>(GoToPath),
            new TypeUserCommandHandler<GoToProviderCommand>(GoToProvider),
            new TypeUserCommandHandler<GoToRootCommand>(GoToRoot),
            new TypeUserCommandHandler<GoUpCommand>(GoUp),
            new TypeUserCommandHandler<MoveCursorDownCommand>(MoveCursorDown),
            new TypeUserCommandHandler<MoveCursorDownPageCommand>(MoveCursorDownPage),
            new TypeUserCommandHandler<MoveCursorToFirstCommand>(MoveCursorToFirst),
            new TypeUserCommandHandler<MoveCursorToLastCommand>(MoveCursorToLast),
            new TypeUserCommandHandler<MoveCursorUpCommand>(MoveCursorUp),
            new TypeUserCommandHandler<MoveCursorUpPageCommand>(MoveCursorUpPage),
            new TypeUserCommandHandler<NewTabCommand>(NewTabAsync),
            new TypeUserCommandHandler<OpenCommandPaletteCommand>(OpenCommandPalette),
            new TypeUserCommandHandler<OpenContainerCommand>(OpenContainer),
            new TypeUserCommandHandler<OpenSelectedCommand>(OpenSelected),
            new TypeUserCommandHandler<RunOrOpenCommand>(RunOrOpen),
            new TypeUserCommandHandler<RefreshCommand>(Refresh),
            new TypeUserCommandHandler<SelectNextTabCommand>(SelectNextTab),
            new TypeUserCommandHandler<SelectPreviousTabCommand>(SelectPreviousTab),
            new TypeUserCommandHandler<SwitchToTabCommand>(SwitchToTab),
        });
    }

    private async Task GoBack()
    {
        if (_selectedTab?.Tab is null) return;

        await _selectedTab.Tab.GoBackAsync();
    }

    private async Task GoForward()
    {
        if (_selectedTab?.Tab is null) return;

        await _selectedTab.Tab.GoForwardAsync();
    }

    private async Task RunOrOpen(RunOrOpenCommand command)
    {
        var item = command.Item ?? _currentSelectedItem?.Value;
        if (item is IContainerViewModel)
        {
            await OpenSelected();
        }
        else if (
            item is IElementViewModel
            {
                Element: {NativePath: not null, Provider: ILocalContentProvider} localFile
            }
        )
        {
            var processStartInfo = new ProcessStartInfo(localFile.NativePath!.Path) {UseShellExecute = true};
            Process.Start(processStartInfo);

            if (_viewMode == ViewMode.RapidTravel)
            {
                await ExitRapidTravel();
            }
        }
    }

    private Task OpenCommandPalette()
    {
        _commandPaletteService.OpenCommandPalette();
        return Task.CompletedTask;
    }

    private async Task GoByFrequency()
        => await _frequencyNavigationService.OpenNavigationWindow();

    private async Task GoToPath()
    {
        var pathInput = new TextInputElement("Path");
        var acceptedForm = await _userCommunicationService.ReadInputs(pathInput);

        if (!acceptedForm) return;

        var path = pathInput.Value!;
        IItem? resolvedPath = null;
        try
        {
            resolvedPath = await _timelessContentProvider.GetItemByNativePathAsync(new NativePath(path));
        }
        catch
        {
            // ignored
        }

        if (resolvedPath is IContainer container)
        {
            await _userCommandHandlerService.HandleCommandAsync(
                new OpenContainerCommand(new AbsolutePath(_timelessContentProvider, container)));
        }
        else if (resolvedPath is IElement {Parent: { } parent})
        {
            await _userCommandHandlerService.HandleCommandAsync(
                new OpenContainerCommand(parent));
        }
        else
        {
            await _userCommunicationService.ShowMessageBox(
                $"Path does not exists: {path}",
                okText: "Ok",
                showCancel: false
            );
        }
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
        var root = _currentLocation?.Value;
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
        if (_currentLocation?.Value is null) return;

        await _userCommandHandlerService.HandleCommandAsync(
            new OpenContainerCommand(new AbsolutePath(_timelessContentProvider, _currentLocation.Value.Provider)));
    }

    private async Task Refresh()
    {
        if (_currentLocation?.Value?.FullName is null) return;
        var refreshedItem =
            await _timelessContentProvider.GetItemByFullNameAsync(_currentLocation.Value.FullName, PointInTime.Present);

        if (refreshedItem is not IContainer refreshedContainer) return;


        if (_selectedTab?.Tab is { } tab)
        {
            await tab.ForceSetCurrentLocation(refreshedContainer);
        }
    }

    private async Task OpenContainer(OpenContainerCommand command)
    {
        var resolvedPath = await command.Path.ResolveAsync();
        if (resolvedPath is not IContainer resolvedContainer) return;

        if (_selectedTab?.Tab is { } tab)
        {
            await tab.SetCurrentLocation(resolvedContainer);
        }
    }

    private async Task OpenSelected()
    {
        var targetContainer = await GetSubContainer();

        if (targetContainer is null
            && _currentSelectedItem?.Value is IContainerViewModel {Container: { } container})
        {
            targetContainer = container;
        }

        if (targetContainer is null)
        {
            return;
        }

        await _appState.SetRapidTravelTextAsync("");
        if (_selectedTab?.Tab is { } tab)
        {
            await tab.SetCurrentLocation(targetContainer);
        }

        async Task<IContainer?> GetSubContainer()
        {
            if (_currentSelectedItem?.Value is not {BaseItem: IElement element})
            {
                return null;
            }

            var subContentProvider = await _contentProviderRegistry.GetSubContentProviderForElement(element);
            if (subContentProvider is null) return null;

            var resolvedItem = await subContentProvider.GetItemByFullNameAsync(
                element,
                new FullName(""),
                _timelessContentProvider.CurrentPointInTime.Value!);
            return resolvedItem as IContainer;
        }
    }

    private async Task GoUp()
    {
        if (_currentLocation?.Value?.Parent is not { } parentPath ||
            await parentPath.ResolveAsyncSafe() is not IContainer newContainer)
        {
            return;
        }

        await _appState.SetRapidTravelTextAsync("");
        if (_selectedTab?.Tab is { } tab)
        {
            await tab.SetCurrentLocation(newContainer);
        }
    }

    private async Task MoveCursorDown()
        => await SelectNewSelectedItem(items =>
        {
            if (_currentSelectedItem?.Value == null) return items.FirstOrDefault();
            return items.SkipWhile(i => !i.EqualsTo(_currentSelectedItem?.Value)).Skip(1).FirstOrDefault();
        });

    private async Task MoveCursorUp()
        => await SelectNewSelectedItem(items =>
        {
            if (_currentSelectedItem?.Value == null) return items.LastOrDefault();
            return items.TakeWhile(i => !i.EqualsTo(_currentSelectedItem?.Value)).LastOrDefault();
        });

    private async Task MoveCursorDownPage()
        => await SelectNewSelectedItem(items =>
        {
            var relevantItems = _currentSelectedItem?.Value is null
                ? items.ToList()
                : items.SkipWhile(i => !i.EqualsTo(_currentSelectedItem.Value)).ToList();

            var fallBackItems = relevantItems.Take(PageSize + 1).Reverse();
            var preferredItems = relevantItems.Skip(PageSize + 1);

            return preferredItems.Concat(fallBackItems).FirstOrDefault();
        });

    private async Task MoveCursorUpPage()
        => await SelectNewSelectedItem(items =>
        {
            var relevantItems = _currentSelectedItem?.Value is null
                ? items.Reverse().ToList()
                : items.TakeWhile(i => !i.EqualsTo(_currentSelectedItem?.Value)).Reverse().ToList();

            var fallBackItems = relevantItems.Take(PageSize).Reverse();
            var preferredItems = relevantItems.Skip(PageSize);

            return preferredItems.Concat(fallBackItems).FirstOrDefault();
        });

    private async Task MoveCursorToFirst()
        => await SelectNewSelectedItem(items => items.FirstOrDefault());

    private async Task MoveCursorToLast()
        => await SelectNewSelectedItem(items => items.LastOrDefault());

    private Task SelectNewSelectedItem(Func<IEnumerable<IItemViewModel>, IItemViewModel?> getNewSelected)
    {
        if (_selectedTab is null || _currentItems?.Value is null) return Task.CompletedTask;

        var newSelectedItem = getNewSelected(_currentItems.Value.ToArray());
        if (newSelectedItem == null) return Task.CompletedTask;

        if (_selectedTab.Tab is { } tab)
        {
            tab.SetSelectedItem(newSelectedItem.ToAbsolutePath(_timelessContentProvider));
        }

        return Task.CompletedTask;
    }

    private Task EnterRapidTravel()
    {
        _appState.SwitchViewModeAsync(ViewMode.RapidTravel);
        return Task.CompletedTask;
    }

    private Task ExitRapidTravel()
    {
        _appState.SwitchViewModeAsync(ViewMode.Default);
        return Task.CompletedTask;
    }

    private async Task SwitchToTab(SwitchToTabCommand command)
    {
        var tabNumber = command.TabNumber;
        var tabViewModel = _appState.Tabs.FirstOrDefault(t => t.TabNumber == tabNumber);

        if (tabNumber == -1)
        {
            var greatestNumber = _appState.Tabs.Max(t => t.TabNumber);
            tabViewModel = _appState.Tabs.FirstOrDefault(t => t.TabNumber == greatestNumber);
        }

        if (tabViewModel == null)
        {
            var newLocation = await GetLocationForNewTabAsync(tabNumber);
            tabViewModel = await CreateTabAsync(newLocation, tabNumber);
        }

        if (_viewMode == ViewMode.RapidTravel)
        {
            await _userCommandHandlerService.HandleCommandAsync(ExitRapidTravelCommand.Instance);
        }

        await _appState.SetSelectedTabAsync(tabViewModel);
    }

    private async Task NewTabAsync(NewTabCommand command)
    {
        var numbers = _appState.Tabs.Select(t => t.TabNumber).ToHashSet();

        var tabNumber = 1;
        while (numbers.Contains(tabNumber))
        {
            tabNumber++;
        }

        IContainer? newLocation = null;

        if (command.Path is { } path)
        {
            newLocation = await _timelessContentProvider.GetItemByFullNameAsync(path, PointInTime.Present) as IContainer;
        }

        newLocation ??= await GetLocationForNewTabAsync(tabNumber);
        var tabViewModel = await CreateTabAsync(newLocation, tabNumber);

        if (_viewMode == ViewMode.RapidTravel)
        {
            await _userCommandHandlerService.HandleCommandAsync(ExitRapidTravelCommand.Instance);
        }

        if (command.Open)
        {
            await _appState.SetSelectedTabAsync(tabViewModel);
        }
    }

    private async Task SelectNextTab()
    {
        var currentTabNumber = _appState.SelectedTab.Value?.TabNumber;

        var nextTabNumbers = _appState.Tabs.Select(t => t.TabNumber).Order().SkipWhile(n => n <= currentTabNumber).ToArray();

        if (nextTabNumbers.Length == 0) return;

        var nextTabNumber = nextTabNumbers[0];
        var tabViewModel = _appState.Tabs.FirstOrDefault(t => t.TabNumber == nextTabNumber);
        await _appState.SetSelectedTabAsync(tabViewModel!);
    }

    private async Task SelectPreviousTab()
    {
        var currentTabNumber = _appState.SelectedTab.Value?.TabNumber;

        var nextTabNumbers = _appState.Tabs.Select(t => t.TabNumber).Order().TakeWhile(n => n < currentTabNumber).ToArray();

        if (nextTabNumbers.Length == 0) return;

        var nextTabNumber = nextTabNumbers[^1];
        var tabViewModel = _appState.Tabs.FirstOrDefault(t => t.TabNumber == nextTabNumber);
        await _appState.SetSelectedTabAsync(tabViewModel!);
    }

    private async Task<IContainer> GetLocationForNewTabAsync(int tabNumber)
    {
        try
        {
            var newLocation = _currentLocation?.Value?.FullName is { } fullName
                ? (IContainer) await _timelessContentProvider.GetItemByFullNameAsync(fullName, PointInTime.Present)
                : _localContentProvider;

            return newLocation;
        }
        catch (Exception ex)
        {
            var fullName = _currentLocation?.Value?.FullName?.Path ?? "unknown";
            _logger.LogError(ex, "Could not resolve container while switching to tab {TabNumber} to path {FullName}", tabNumber, fullName);
        }

        return _localContentProvider;
    }

    private async Task<ITabViewModel> CreateTabAsync(IContainer newLocation, int tabNumber)
    {
        var tab = await _serviceProvider.GetAsyncInitableResolver(newLocation)
            .GetRequiredServiceAsync<ITab>();
        var newTabViewModel = _serviceProvider.GetInitableResolver(tab, tabNumber).GetRequiredService<ITabViewModel>();

        _appState.AddTab(newTabViewModel);

        return newTabViewModel;
    }

    private Task CloseTab()
    {
        if ((!_applicationConfiguration.AllowCloseLastTab && _appState.Tabs.Count < 2) || _selectedTab == null)
        {
            return Task.CompletedTask;
        }

        var tabToRemove = _selectedTab;
        _appState.RemoveTab(tabToRemove!);

        try
        {
            tabToRemove.Dispose();
        }
        catch
        {
            // ignored
        }

        return Task.CompletedTask;
    }
}