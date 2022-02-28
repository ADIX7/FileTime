using MvvmGen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using FileTime.App.Core.Tab;
using System.Threading.Tasks;
using FileTime.Core.Models;
using System.Threading;
using FileTime.Avalonia.Configuration;
using FileTime.Avalonia.Misc;
using FileTime.Core.Extensions;
using FileTime.Avalonia.ViewModels;
using FileTime.Providers.Favorites;

namespace FileTime.Avalonia.Application
{
    [ViewModel]
    public partial class AppState
    {
        [Property]
        private ObservableCollection<TabContainer> _tabs = new();

        [Property]
        [PropertyCallMethod(nameof(SelectedTabChanged))]
        private TabContainer _selectedTab;

        [Property]
        private ViewMode _viewMode;

        [Property]
        private string _rapidTravelText = "";

        [Property]
        private List<CommandBindingConfiguration> _possibleCommands = new();

        [Property]
        private List<InputElementWrapper> _inputs;

        [Property]
        private string _messageBoxText;

        [Property]
        private ObservableCollection<string> _popupTexts = new();

        [Property]
        private bool _isAllShortcutVisible;

        [Property]
        private bool _noCommandFound;

        [Property]
        private List<IItem> _favoriteElements;

        public List<KeyConfig> PreviousKeys { get; } = new();
        
        public ObservableCollection<ParallelCommandsViewModel> TimelineCommands { get; } = new();

        partial void OnInitialize()
        {
            _tabs.CollectionChanged += TabsChanged;
        }

        private void TabsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            SelectedTab ??= Tabs.Count > 0 ? Tabs[0] : null;

            List<TabContainer> itemsAdded = new();
            List<TabContainer> itemsRemoved = new();
            if (e.NewItems != null && e.OldItems != null)
            {
                itemsAdded.AddRange(e.NewItems.Cast<TabContainer>().Except(e.OldItems.Cast<TabContainer>()));
                itemsRemoved.AddRange(e.OldItems.Cast<TabContainer>().Except(e.NewItems.Cast<TabContainer>()));
            }
            else if (e.NewItems != null)
            {
                itemsAdded.AddRange(e.NewItems.Cast<TabContainer>());
            }
            else if (e.OldItems != null)
            {
                itemsRemoved.AddRange(e.OldItems.Cast<TabContainer>());
            }

            foreach (var item in itemsAdded)
            {
                item.TabState.ItemMarked.Add(TabItemMarked);
                item.TabState.ItemUnmarked.Add(TabItemUnmarked);
            }

            foreach (var item in itemsRemoved)
            {
                item.TabState.ItemMarked.Remove(TabItemMarked);
                item.TabState.ItemUnmarked.Remove(TabItemUnmarked);
            }
        }

        private void SelectedTabChanged()
        {
            foreach (var tab in Tabs)
            {
                tab.IsSelected = tab == SelectedTab;
            }
        }

        private async Task TabItemMarked(TabState tabState, AbsolutePath item, CancellationToken token = default)
        {
            var tabContainer = Tabs.FirstOrDefault(t => t.TabState == tabState);
            if (tabContainer != null)
            {
                var item2 = (await tabContainer.CurrentLocation.GetItems(token)).FirstOrDefault(i => i.Item.FullName == item.Path);
                if (token.IsCancellationRequested) return;
                if (item2 != null)
                {
                    item2.IsMarked = true;
                }
            }
        }

        private async Task TabItemUnmarked(TabState tabState, AbsolutePath item, CancellationToken token = default)
        {
            var tabContainer = Tabs.FirstOrDefault(t => t.TabState == tabState);
            if (tabContainer != null)
            {
                var item2 = (await tabContainer.CurrentLocation.GetItems(token)).FirstOrDefault(i => i.Item.FullName == item.Path);
                if (token.IsCancellationRequested) return;
                if (item2 != null)
                {
                    item2.IsMarked = false;
                }
            }
        }

        public async Task ExitRapidTravelMode()
        {
            ViewMode = ViewMode.Default;

            PreviousKeys.Clear();
            PossibleCommands = new();
            RapidTravelText = "";

            await SelectedTab.OpenContainer(await SelectedTab.CurrentLocation.Container.WithoutVirtualContainer(MainPageViewModel.RAPIDTRAVEL));
        }
    }
}
