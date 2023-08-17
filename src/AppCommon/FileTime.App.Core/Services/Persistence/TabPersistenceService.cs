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

    public TabPersistenceService(
        IApplicationSettings applicationSettings,
        IAppState appState,
        ITimelessContentProvider timelessContentProvider,
        IServiceProvider serviceProvider,
        ILocalContentProvider localContentProvider,
        TabPersistenceSettings tabPersistenceSettings,
        ILogger<TabPersistenceService> logger)
    {
        _appState = appState;
        _logger = logger;
        _settingsPath = Path.Combine(applicationSettings.AppDataRoot, "savedState.json");
        _timelessContentProvider = timelessContentProvider;
        _serviceProvider = serviceProvider;
        _localContentProvider = localContentProvider;
        _tabPersistenceSettings = tabPersistenceSettings;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    public async Task InitAsync()
        => await LoadStatesAsync();

    public Task ExitAsync(CancellationToken token = default)
    {
        if(!_tabPersistenceSettings.SaveState) return Task.CompletedTask;
        SaveStates(token);

        return Task.CompletedTask;
    }

    private async Task LoadStatesAsync(CancellationToken token = default)
    {
        if (!File.Exists(_settingsPath) || !_tabPersistenceSettings.LoadState)
        {
            await CreateEmptyTab();
            return;
        }

        try
        {
            await using var stateReader = File.OpenRead(_settingsPath);
            var state = await JsonSerializer.DeserializeAsync<PersistenceRoot>(stateReader, cancellationToken: token);
            if (state != null)
            {
                if (await RestoreTabs(state.TabStates)) return;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unknown exception while restoring app state");
        }

        await CreateEmptyTab();

        async Task CreateEmptyTab()
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

            var tab = await _serviceProvider.GetAsyncInitableResolver<IContainer>(currentDirectory ?? _localContentProvider)
                .GetRequiredServiceAsync<ITab>();
            var tabViewModel = _serviceProvider.GetInitableResolver(tab, 1).GetRequiredService<ITabViewModel>();

            _appState.AddTab(tabViewModel);
        }
    }

    private async Task<bool> RestoreTabs(TabStates? tabStates)
    {
        if (tabStates == null
            || tabStates.Tabs == null)
        {
            return false;
        }

        try
        {
            foreach (var tab in tabStates.Tabs)
            {
                try
                {
                    if (tab.Path == null) continue;
                    if (_contentProvidersNotToRestore.Any(p => tab.Path.StartsWith(p))) continue;

                    IContainer? container = null;
                    var path = FullName.CreateSafe(tab.Path);
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
                            path = path?.GetParent();
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
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unknown exception while restoring tabs");
            return false;
        }

        if (_appState.Tabs.Count == 0) return false;

        var optimalTabs = _appState
            .Tabs
            .TakeWhile(t => t.TabNumber <= tabStates.ActiveTabNumber)
            .Reverse();
        var suboptimalTabs = _appState
            .Tabs
            .SkipWhile(t => t.TabNumber <= tabStates.ActiveTabNumber);

        var tabToActivate = optimalTabs.Concat(suboptimalTabs).FirstOrDefault();
        if (tabToActivate is not null) await _appState.SetSelectedTabAsync(tabToActivate);

        return true;
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
            if (currentLocation is null) continue;
            var path = currentLocation.FullName!.Path;

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