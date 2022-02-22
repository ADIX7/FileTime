using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FileTime.Avalonia.Application;
using FileTime.Avalonia.Models.Persistence;
using System;
using FileTime.Core.Components;
using FileTime.Core.Providers;
using FileTime.Providers.Local;
using FileTime.Core.Models;
using Microsoft.Extensions.Logging;
using FileTime.Core.Timeline;
using FileTime.Core.Services;

namespace FileTime.Avalonia.Services
{
    public class StatePersistenceService
    {
        private readonly AppState _appState;
        private readonly ItemNameConverterService _itemNameConverterService;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _settingsPath;
        private readonly IEnumerable<IContentProvider> _contentProviders;
        private readonly LocalContentProvider _localContentProvider;
        private readonly ILogger<StatePersistenceService> _logger;
        private readonly TimeRunner _timeRunner;
        private readonly IServiceProvider _serviceProvider;

        public StatePersistenceService(
            AppState appState,
            ItemNameConverterService itemNameConverterService,
            IEnumerable<IContentProvider> contentProviders,
            LocalContentProvider localContentProvider,
            ILogger<StatePersistenceService> logger,
            TimeRunner timeRunner,
            IServiceProvider serviceProvider)
        {
            _appState = appState;
            _itemNameConverterService = itemNameConverterService;
            _contentProviders = contentProviders;
            _localContentProvider = localContentProvider;
            _logger = logger;
            _settingsPath = Path.Combine(Program.AppDataRoot, "savedState.json");
            _timeRunner = timeRunner;
            _serviceProvider = serviceProvider;

            _jsonOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        public async Task LoadStatesAsync()
        {
            if (!File.Exists(_settingsPath)) return;

            try
            {
                using var stateReader = File.OpenRead(_settingsPath);
                var state = await JsonSerializer.DeserializeAsync<PersistenceRoot>(stateReader);
                if (state != null)
                {
                    await RestoreTabs(state.TabStates);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unkown exception while restoring app state.");
            }
        }

        public void SaveStates()
        {
            var state = new PersistenceRoot
            {
                TabStates = SerializeTabStates()
            };
            var settingsDirectory = new DirectoryInfo(string.Join(Path.DirectorySeparatorChar, _settingsPath.Split(Path.DirectorySeparatorChar)[0..^1]));
            if (!settingsDirectory.Exists) settingsDirectory.Create();
            var serializedData = JsonSerializer.Serialize(state, _jsonOptions);
            File.WriteAllText(_settingsPath, serializedData);
        }

        private TabStates SerializeTabStates()
        {
            var tabStates = new List<TabState>();
            foreach (var tab in _appState.Tabs)
            {
                tabStates.Add(new TabState(tab));
            }

            return new TabStates()
            {
                Tabs = tabStates,
                ActiveTabNumber = _appState.SelectedTab.TabNumber
            };
        }

        private async Task<bool> RestoreTabs(TabStates? tabStates)
        {
            try
            {
                if (tabStates == null
                    || tabStates.Tabs == null)
                {
                    return false;
                }

                foreach (var tab in tabStates.Tabs)
                {
                    try
                    {
                        if (tab.Path == null) continue;

                        IItem? pathItem = null;
                        if (tab.Path.StartsWith(Constants.ContentProviderProtocol))
                        {
                            var contentProviderName = tab.Path.Substring(Constants.ContentProviderProtocol.Length);
                            pathItem = _contentProviders.FirstOrDefault(c => c.Name == contentProviderName);
                        }
                        else
                        {
                            foreach (var contentProvider in _contentProviders)
                            {
                                if (await contentProvider.CanHandlePath(tab.Path))
                                {
                                    pathItem = await contentProvider.GetByPath(tab.Path, true);
                                    if (pathItem != null) break;
                                }
                            }
                        }

                        var container = pathItem switch
                        {
                            IContainer c => c,
                            IElement e => e.GetParent(),
                            _ => null
                        };

                        if (container == null) continue;

                        var newTab = new Tab();
                        while (true)
                        {
                            try
                            {
                                if (container == null) throw new Exception($"Could not find an initializable path along {tab.Path}");
                                await newTab.Init(container);
                                break;
                            }
                            catch
                            {
                                container = container!.GetParent();
                            }
                        }

                        var newTabContainer = new TabContainer(_serviceProvider, _timeRunner, newTab, _localContentProvider, _itemNameConverterService, _appState);
                        await newTabContainer.Init(tab.Number);
                        _appState.Tabs.Add(newTabContainer);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Unkown exception while restoring tab. {TabState}", JsonSerializer.Serialize(tab, _jsonOptions));
                    }
                }

                if (_appState.Tabs.FirstOrDefault(t => t.TabNumber == tabStates.ActiveTabNumber) is TabContainer tabContainer)
                {
                    _appState.SelectedTab = tabContainer;
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unkown exception while restoring tabs.");
            }
            return false;
        }
    }
}