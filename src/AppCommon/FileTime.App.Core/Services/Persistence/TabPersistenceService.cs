using System.Text.Json;
using FileTime.App.Core.Models;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
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

    private class PersistenceRoot
    {
        public TabStates? TabStates { get; set; }
    }

    private class TabStates
    {
        public List<TabState>? Tabs { get; set; }
        public int? ActiveTabNumber { get; set; }
    }

    private class TabState
    {
        public string? Path { get; set; }
        public int Number { get; set; }

        public TabState()
        {
        }

        public TabState(FullName path, int number)
        {
            Path = path.Path;
            Number = number;
        }
    }

    private readonly string _settingsPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILocalContentProvider _localContentProvider;

    public TabPersistenceService(
        IApplicationSettings applicationSettings,
        IAppState appState,
        ITimelessContentProvider timelessContentProvider,
        IServiceProvider serviceProvider,
        ILocalContentProvider localContentProvider,
        ILogger<TabPersistenceService> logger)
    {
        _appState = appState;
        _logger = logger;
        _settingsPath = Path.Combine(applicationSettings.AppDataRoot, "savedState.json");
        _timelessContentProvider = timelessContentProvider;
        _serviceProvider = serviceProvider;
        _localContentProvider = localContentProvider;

        _jsonOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
    }

    public Task ExitAsync()
    {
        SaveStates();

        return Task.CompletedTask;
    }

    private async Task LoadStatesAsync()
    {
        if (!File.Exists(_settingsPath))
        {
            CreateEmptyTab();
            return;
        }

        try
        {
            await using var stateReader = File.OpenRead(_settingsPath);
            var state = await JsonSerializer.DeserializeAsync<PersistenceRoot>(stateReader);
            if (state != null)
            {
                if (await RestoreTabs(state.TabStates)) return;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unknown exception while restoring app state");
        }

        CreateEmptyTab();

        void CreateEmptyTab()
        {
            var tab = _serviceProvider.GetInitableResolver<IContainer>(_localContentProvider)
                .GetRequiredService<ITab>();
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

                    IContainer? container = null;
                    var path = new FullName(tab.Path);
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

                    var tabToLoad = _serviceProvider.GetInitableResolver(container)
                        .GetRequiredService<ITab>();
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

        var tabToActivate = _appState.Tabs.FirstOrDefault(t => t.TabNumber == tabStates.ActiveTabNumber);
        if (tabToActivate is not null) _appState.SetSelectedTab(tabToActivate);

        return true;
    }

    public void SaveStates()
    {
        var state = new PersistenceRoot
        {
            TabStates = SerializeTabStates()
        };
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
            var currentLocation = tab.CachedCurrentLocation;
            if (currentLocation is null) continue;
            tabStates.Add(new TabState(currentLocation.FullName!, tab.TabNumber));
        }

        return new TabStates()
        {
            Tabs = tabStates,
            ActiveTabNumber = _appState.CurrentSelectedTab?.TabNumber
        };
    }

    public async Task InitAsync()
    {
        await LoadStatesAsync();
    }
}