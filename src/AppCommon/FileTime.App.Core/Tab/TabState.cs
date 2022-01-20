using System.Collections.ObjectModel;
using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.App.Core.Tab
{
    public class TabState
    {
        private readonly Dictionary<IContainer, List<TabItem>> _selectedItems;
        private readonly Dictionary<IContainer, IReadOnlyList<TabItem>> _selectedItemsReadOnly;
        public IReadOnlyDictionary<IContainer, IReadOnlyList<TabItem>> SelectedItems { get; }

        public FileTime.Core.Components.Tab Tab { get; }

        public TabState(FileTime.Core.Components.Tab pane)
        {
            Tab = pane;

            _selectedItems = new Dictionary<IContainer, List<TabItem>>();
            _selectedItemsReadOnly = new Dictionary<IContainer, IReadOnlyList<TabItem>>();
            SelectedItems = new ReadOnlyDictionary<IContainer, IReadOnlyList<TabItem>>(_selectedItemsReadOnly);
        }

        public void AddSelectedItem(IContentProvider contentProvider, IContainer container, string path)
        {
            if (!_selectedItems.ContainsKey(container))
            {
                var val = new List<TabItem>();
                _selectedItems.Add(container, val);
                _selectedItemsReadOnly.Add(container, val.AsReadOnly());
            }

            foreach (var content in _selectedItems[container])
            {
                if (content.ContentProvider == contentProvider && content.Path == path) return;
            }

            _selectedItems[container].Add(new TabItem(contentProvider, path));
        }

        public void RemoveSelectedItem(IContentProvider contentProvider, IContainer container, string path)
        {
            if (_selectedItems.ContainsKey(container))
            {
                var selectedItems = _selectedItems[container];
                for (var i = 0; i < selectedItems.Count; i++)
                {
                    if (selectedItems[i].ContentProvider == contentProvider && selectedItems[i].Path == path)
                    {
                        selectedItems.RemoveAt(i--);
                    }
                }
            }
        }

        public bool ContainsSelectedItem(IContentProvider contentProvider, IContainer container, string path)
        {
            if (!_selectedItems.ContainsKey(container)) return false;

            foreach (var content in _selectedItems[container])
            {
                if (content.ContentProvider == contentProvider && content.Path == path) return true;
            }

            return false;
        }

        public async Task<IReadOnlyList<TabItem>> GetCurrentSelectedItems()
        {
            var currentLocation = await Tab.GetCurrentLocation();

            return SelectedItems.ContainsKey(currentLocation)
                ? SelectedItems[currentLocation]
                : new List<TabItem>().AsReadOnly();
        }
    }
}