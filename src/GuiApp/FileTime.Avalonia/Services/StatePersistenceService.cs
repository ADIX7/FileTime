using System.Linq;
using System.Net;
using System.Text;
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

        public StatePersistenceService(
            AppState appState,
            ItemNameConverterService itemNameConverterService,
            IEnumerable<IContentProvider> contentProviders,
            LocalContentProvider localContentProvider)
        {
            _appState = appState;
            _itemNameConverterService = itemNameConverterService;
            _contentProviders = contentProviders;
            _localContentProvider = localContentProvider;
            _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileTime", "savedState.json");

            _jsonOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task LoadStatesAsync()
        {
            if (!File.Exists(_settingsPath)) return;

            using var stateReader = File.OpenRead(_settingsPath);
            var state = await JsonSerializer.DeserializeAsync<PersistenceRoot>(stateReader);
            if (state != null)
            {
                await RestoreTabs(state.TabStates);
            }
        }

        public async Task SaveStatesAsync()
        {
            var state = new PersistenceRoot
            {
                TabStates = SerializeTabStates()
            };
            var settingsDirectory = new DirectoryInfo(string.Join(Path.DirectorySeparatorChar, _settingsPath.Split(Path.DirectorySeparatorChar)[0..^1]));
            if (!settingsDirectory.Exists) settingsDirectory.Create();
            using var stateWriter = File.OpenWrite(_settingsPath);
            await JsonSerializer.SerializeAsync(stateWriter, state, _jsonOptions);
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
            if (tabStates == null
                || tabStates.Tabs == null)
            {
                return false;
            }

            foreach (var tab in tabStates.Tabs)
            {
                if (tab.Path == null) continue;

                IItem? pathItem = null;
                foreach (var contentProvider in _contentProviders)
                {
                    if (contentProvider.CanHandlePath(tab.Path))
                    {
                        pathItem = await contentProvider.GetByPath(tab.Path, true);
                        if (pathItem != null) break;
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
                await newTab.Init(container);

                var newTabContainer = new TabContainer(newTab, _localContentProvider, _itemNameConverterService);
                await newTabContainer.Init(tab.Number);
                _appState.Tabs.Add(newTabContainer);
            }

            if (_appState.Tabs.FirstOrDefault(t => t.TabNumber == tabStates.ActiveTabNumber) is TabContainer tabContainer)
            {
                _appState.SelectedTab = tabContainer;
            }

            return true;
        }
    }
}