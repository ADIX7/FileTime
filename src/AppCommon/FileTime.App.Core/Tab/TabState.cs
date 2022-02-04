using System.Collections.ObjectModel;
using AsyncEvent;
using FileTime.Core.Models;

namespace FileTime.App.Core.Tab
{
    public class TabState
    {
        private readonly Dictionary<IContainer, List<AbsolutePath>> _markedItems;
        private readonly Dictionary<IContainer, IReadOnlyList<AbsolutePath>> _markedItemsReadOnly;
        public IReadOnlyDictionary<IContainer, IReadOnlyList<AbsolutePath>> MarkedItems { get; }

        public FileTime.Core.Components.Tab Tab { get; }

        public AsyncEventHandler<TabState, AbsolutePath> ItemMarked { get; } = new();
        public AsyncEventHandler<TabState, AbsolutePath> ItemUnmarked { get; } = new();

        public TabState(FileTime.Core.Components.Tab pane)
        {
            Tab = pane;

            _markedItems = new Dictionary<IContainer, List<AbsolutePath>>();
            _markedItemsReadOnly = new Dictionary<IContainer, IReadOnlyList<AbsolutePath>>();
            MarkedItems = new ReadOnlyDictionary<IContainer, IReadOnlyList<AbsolutePath>>(_markedItemsReadOnly);
        }

        public async Task AddMarkedItem(IContainer container, AbsolutePath path)
        {
            if (!_markedItems.ContainsKey(container))
            {
                var val = new List<AbsolutePath>();
                _markedItems.Add(container, val);
                _markedItemsReadOnly.Add(container, val.AsReadOnly());
            }

            foreach (var content in _markedItems[container])
            {
                if (content.Equals(path)) return;
            }

            var tabItem = new AbsolutePath(path);
            _markedItems[container].Add(tabItem);
            await ItemMarked.InvokeAsync(this, tabItem);
        }

        public async Task RemoveMarkedItem(IContainer container, AbsolutePath path)
        {
            if (_markedItems.ContainsKey(container))
            {
                var markedItems = _markedItems[container];
                for (var i = 0; i < markedItems.Count; i++)
                {
                    if (markedItems[i].Equals(path))
                    {
                        await ItemUnmarked.InvokeAsync(this, markedItems[i]);
                        markedItems.RemoveAt(i--);
                    }
                }
            }
        }

        public async Task ClearMarkedItems(IContainer container)
        {
            if (_markedItems.ContainsKey(container))
            {
                var markedItems = _markedItems[container];
                for (var i = 0; i < markedItems.Count; i++)
                {
                    await ItemUnmarked.InvokeAsync(this, markedItems[i]);
                    markedItems.RemoveAt(i--);
                }
            }
        }

        public async Task ClearCurrentMarkedItems()
        {
            await ClearMarkedItems(await Tab.GetCurrentLocation());
        }

        public bool ContainsMarkedItem(IContainer container, AbsolutePath path)
        {
            if (!_markedItems.ContainsKey(container)) return false;

            foreach (var content in _markedItems[container])
            {
                if (content.Equals(path)) return true;
            }

            return false;
        }

        public async Task<IReadOnlyList<AbsolutePath>> GetCurrentMarkedItems()
        {
            return GetCurrentMarkedItems(await Tab.GetCurrentLocation());
        }

        public IReadOnlyList<AbsolutePath> GetCurrentMarkedItems(IContainer container)
        {
            return MarkedItems.ContainsKey(container)
                ? MarkedItems[container]
                : new List<AbsolutePath>().AsReadOnly();
        }

        public async Task MarkCurrentItem()
        {
            var currentLocation = await Tab!.GetCurrentLocation();
            if (currentLocation != null)
            {
                var currentSelectedItem = await Tab.GetCurrentSelectedItem()!;
                if (currentSelectedItem != null)
                {
                    if (ContainsMarkedItem(currentLocation, new AbsolutePath(currentSelectedItem)))
                    {
                        await RemoveMarkedItem(currentLocation, new AbsolutePath(currentSelectedItem));
                    }
                    else
                    {
                        await AddMarkedItem(currentLocation, new AbsolutePath(currentSelectedItem));
                    }
                }

                await Tab.SelectNextItem();
            }
        }
    }
}