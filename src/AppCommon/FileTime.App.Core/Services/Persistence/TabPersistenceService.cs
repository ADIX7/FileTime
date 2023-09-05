using System.Text.Json;
using FileTime.App.Core.Configuration;
using FileTime.App.Core.Models;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.Core.Models.Extensions;
using FileTime.Core.Services;
using FileTime.Core.Timeline;
using FileTime.Providers.Local;
using InitableService;
using Microsoft.Extensions.Logging;

namespace FileTime.App.Core.Services.Persistence;

public class TabPersistenceService : ITabPersistenceService
{
    private readonly IAppState _appState;
    private readonly ILogger<TabPersistenceService> _logger;

    //TODO: make this a configuration maybe?
    private readonly List<string> _contentProvidersNotToRestore = new()
    {
        "search",
        "container-size-scan"
    };

    private record PersistenceRoot(TabStates? TabStates);

    private record TabStates(List<TabState>? Tabs, int? ActiveTabNumber);

    private record TabState(string? Path, int Number);

    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILocalContentProvider _localContentProvider;
    private readonly TabPersistenceSettings _tabPersistenceSettings;
    private readonly TabsToOpenOnStart _tabsToOpen;

    public TabPersistenceService(
        IApplicationSettings applicationSettings,
        IAppState appState,
        ITimelessContentProvider timelessContentProvider,
        IServiceProvider serviceProvider,
        ILocalContentProvider localContentProvider,
        TabPersistenceSettings tabPersistenceSettings,
        TabsToOpenOnStart tabsToOpen,
        ILogger<TabPersistenceService> logger)
    {
        _appState = appState;
        _logger = logger;
        _settingsPath = Path.Combine(applicationSettings.AppDataRoot, "savedState.json");
        _timelessContentProvider = timelessContentProvider;
        _serviceProvider = serviceProvider;
        _localContentProvider = localContentProvider;
        _tabPersistenceSettings = tabPersistenceSettings;
        _tabsToOpen = tabsToOpen;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    public async Task InitAsync()
    {
        var containers = new List<(int? TabNumber, IContainer Container)>();

        foreach (var (requestedTabNumber, nativePath) in _tabsToOpen.TabsToOpen)
        {
            if (await _timelessContentProvider.GetItemByNativePathAsync(nativePath) is not IContainer container)
                continue;

            containers.Add((requestedTabNumber, container));
        }

        var loadedTabViewModels = await LoadStatesAsync(containers.Count == 0);

        var tabViewModels = new List<ITabViewModel>();
        foreach (var (requestedTabNumber, container) in containers)
        {
            var tabNumber = requestedTabNumber ?? 1;

            if (tabNumber < 1) tabNumber = 1;

            var tabNumbers = _appState.Tabs.Select(t => t.TabNumber).ToHashSet();

            while (tabNumbers.Contains(tabNumber))
            {
                tabNumber++;
            }

            var tab = await _serviceProvider.GetAsyncInitableResolver(container)
                .GetRequiredServiceAsync<ITab>();
            var tabViewModel = _serviceProvider.GetInitableResolver(tab, tabNumber).GetRequiredService<ITabViewModel>();

            _appState.AddTab(tabViewModel);
            tabViewModels.Add(tabViewModel);
        }

        tabViewModels.Reverse();
        await _appState.SetSelectedTabAsync(tabViewModels.Concat(loadedTabViewModels).First());
    }

    public Task ExitAsync(CancellationToken token = default)
    {
        if (!_tabPersistenceSettings.SaveState) return Task.CompletedTask;
        SaveStates(token);

        return Task.CompletedTask;
    }

    private async Task<IEnumerable<ITabViewModel>> LoadStatesAsync(
        bool createEmptyIfNecessary,
        CancellationToken token = default)
    {
        if (!File.Exists(_settingsPath) || !_tabPersistenceSettings.LoadState)
        {
            if (createEmptyIfNecessary)
            {
                var tabViewModel = await CreateEmptyTab();
                return new[] { tabViewModel };
            }

            return Enumerable.Empty<ITabViewModel>();
        }

        try
        {
            await using var stateReader = File.OpenRead(_settingsPath);
            var state = await JsonSerializer.DeserializeAsync<PersistenceRoot>(stateReader, cancellationToken: token);
            if (state != null)
            {
                var (success, tabViewModels) = await RestoreTabs(state.TabStates);
                if (success) return tabViewModels;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unknown exception while restoring app state");
        }

        if (createEmptyIfNecessary)
        {
            var tabViewModel = await CreateEmptyTab();
            return new[] { tabViewModel };
        }

        return Enumerable.Empty<ITabViewModel>();

        async Task<ITabViewModel> CreateEmptyTab()
        {
            IContainer? currentDirectory = null;
            try
            {
                currentDirectory = await _localContentProvider.GetItemByNativePathAsync(
                    new NativePath(Environment.CurrentDirectory),
                    PointInTime.Present
                ) as IContainer;
            }
            catch
            {
                // ignored
            }

            var tab = await _serviceProvider
                .GetAsyncInitableResolver(currentDirectory ?? _localContentProvider)
                .GetRequiredServiceAsync<ITab>();
            var tabViewModel = _serviceProvider.GetInitableResolver(tab, 1).GetRequiredService<ITabViewModel>();

            _appState.AddTab(tabViewModel);

            return tabViewModel;
        }
    }

    private async Task<(bool Success, IEnumerable<ITabViewModel>)> RestoreTabs(TabStates? tabStates)
    {
        if (tabStates == null
            || tabStates.Tabs == null)
        {
            return (false, Enumerable.Empty<ITabViewModel>());
        }

        foreach (var tab in tabStates.Tabs)
        {
            try
            {
                if (tab.Path == null) continue;
                if (_contentProvidersNotToRestore.Any(p => tab.Path.StartsWith(p))) continue;

                IContainer? container;
                var path = FullName.CreateSafe(tab.Path)!;
                while (true)
                {
                    try
                    {
                        var pathItem =
                            await _timelessContentProvider.GetItemByFullNameAsync(path, PointInTime.Present);

                        container = pathItem switch
                        {
                            IContainer c => c,
                            IElement e =>
                                e.Parent is null
                                    ? null
                                    : await e.Parent.ResolveAsync() as IContainer,
                            _ => null
                        };
                        break;
                    }
                    catch
                    {
                        path = path.GetParent();
                        if (path == null)
                        {
                            throw new Exception($"Could not find an initializable path along {tab.Path}");
                        }
                    }
                }

                if (container == null) continue;

                if (_contentProvidersNotToRestore.Contains(container.Provider.Name)) continue;

                var tabToLoad = await _serviceProvider.GetAsyncInitableResolver(container)
                    .GetRequiredServiceAsync<ITab>();
                var tabViewModel = _serviceProvider.GetInitableResolver(tabToLoad, tab.Number)
                    .GetRequiredService<ITabViewModel>();

                _appState.AddTab(tabViewModel);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unknown exception while restoring tab. {TabState}",
                    JsonSerializer.Serialize(tab, _jsonOptions));
            }
        }

        if (_appState.Tabs.Count == 0) return (false, Enumerable.Empty<ITabViewModel>());

        var optimalTabs = _appState
            .Tabs
            .TakeWhile(t => t.TabNumber <= tabStates.ActiveTabNumber)
            .Reverse();
        var suboptimalTabs = _appState
            .Tabs
            .SkipWhile(t => t.TabNumber <= tabStates.ActiveTabNumber);

        return (true, optimalTabs.Concat(suboptimalTabs));
    }

    public void SaveStates(CancellationToken token = default)
    {
        var state = new PersistenceRoot(SerializeTabStates());

        var settingsDirectory = new DirectoryInfo(string.Join(Path.DirectorySeparatorChar,
            _settingsPath.Split(Path.DirectorySeparatorChar)[0..^1]));
        if (!settingsDirectory.Exists) settingsDirectory.Create();
        var serializedData = JsonSerializer.Serialize(state, _jsonOptions);
        File.WriteAllText(_settingsPath, serializedData);
    }

    private TabStates SerializeTabStates()
    {
        var tabStates = new List<TabState>();
        foreach (var tab in _appState.Tabs)
        {
            var currentLocation = tab.CurrentLocation.Value;
            if (currentLocation?.FullName?.Path is not { } path) continue;

            if (currentLocation.GetExtension<RealContainerProviderExtension>()?.RealContainer() is { } realPath)
            {
                path = realPath.Path.Path;
            }

            tabStates.Add(new TabState(path, tab.TabNumber));
        }

        return new TabStates(
            tabStates,
            _appState.SelectedTab.Value?.TabNumber
        );
    }
}