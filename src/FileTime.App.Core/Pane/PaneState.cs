using System.Collections.ObjectModel;
using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.App.Core.Pane
{
    public class PaneState
    {
        private readonly Dictionary<IContainer, List<PaneItem>> _selectedItems;
        private readonly Dictionary<IContainer, IReadOnlyList<PaneItem>> _selectedItemsReadOnly;
        public IReadOnlyDictionary<IContainer, IReadOnlyList<PaneItem>> SelectedItems { get; }

        public FileTime.Core.Components.Pane Pane { get; }

        public PaneState(FileTime.Core.Components.Pane pane)
        {
            Pane = pane;

            _selectedItems = new Dictionary<IContainer, List<PaneItem>>();
            _selectedItemsReadOnly = new Dictionary<IContainer, IReadOnlyList<PaneItem>>();
            SelectedItems = new ReadOnlyDictionary<IContainer, IReadOnlyList<PaneItem>>(_selectedItemsReadOnly);
        }

        public void AddSelectedItem(IContentProvider contentProvider, IContainer container, string path)
        {
            if (!_selectedItems.ContainsKey(container))
            {
                var val = new List<PaneItem>();
                _selectedItems.Add(container, val);
                _selectedItemsReadOnly.Add(container, val.AsReadOnly());
            }

            foreach (var content in _selectedItems[container])
            {
                if (content.ContentProvider == contentProvider && content.Path == path) return;
            }

            _selectedItems[container].Add(new PaneItem(contentProvider, path));
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

        public IReadOnlyList<PaneItem> GetCurrentSelectedItems() =>
            SelectedItems.ContainsKey(Pane.CurrentLocation)
            ? SelectedItems[Pane.CurrentLocation]
            : new List<PaneItem>().AsReadOnly();
    }
}