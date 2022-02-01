using MvvmGen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using FileTime.App.Core.Tab;
using System.Threading.Tasks;
using FileTime.Core.Models;

namespace FileTime.Avalonia.Application
{
    [ViewModel]
    public partial class AppState
    {
        [Property]
        private ObservableCollection<TabContainer> _tabs = new();

        [Property]
        private TabContainer _selectedTab;

        [Property]
        private ViewMode _viewMode;

        [Property]
        private string _rapidTravelText = "";

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

        private async Task TabItemMarked(TabState tabState, AbsolutePath item)
        {
            var tabContainer = Tabs.FirstOrDefault(t => t.TabState == tabState);
            if (tabContainer != null)
            {
                var item2 = (await tabContainer.CurrentLocation.GetItems()).FirstOrDefault(i => i.Item.FullName == item.Path);
                if (item2 != null)
                {
                    item2.IsMarked = true;
                }
            }
        }

        private async Task TabItemUnmarked(TabState tabState, AbsolutePath item)
        {
            var tabContainer = Tabs.FirstOrDefault(t => t.TabState == tabState);
            if (tabContainer != null)
            {
                var item2 = (await tabContainer.CurrentLocation.GetItems()).FirstOrDefault(i => i.Item.FullName == item.Path);
                if (item2 != null)
                {
                    item2.IsMarked = false;
                }
            }
        }
    }
}
