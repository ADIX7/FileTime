using System.Buffers;
using System.Collections.ObjectModel;
using DeclarativeProperty;

namespace TerminalUI.Controls;

public class ListView<TDataContext, TItem> : View<TDataContext>
{
    private static readonly ArrayPool<ListViewItem<TItem>> ListViewItemPool = ArrayPool<ListViewItem<TItem>>.Shared;

    private readonly List<IDisposable> _itemsDisposables = new();
    private Func<IEnumerable<TItem>>? _getItems;
    private object? _itemsSource;
    private ListViewItem<TItem>[]? _listViewItems;
    private int _listViewItemLength;

    public object? ItemsSource
    {
        get => _itemsSource;
        set
        {
            if (_itemsSource == value) return;
            _itemsSource = value;

            foreach (var disposable in _itemsDisposables)
            {
                disposable.Dispose();
            }

            _itemsDisposables.Clear();

            if (_itemsSource is IDeclarativeProperty<ObservableCollection<TItem>> observableDeclarativeProperty)
                _getItems = () => observableDeclarativeProperty.Value;
            else if (_itemsSource is IDeclarativeProperty<ReadOnlyObservableCollection<TItem>> readOnlyObservableDeclarativeProperty)
                _getItems = () => readOnlyObservableDeclarativeProperty.Value;
            else if (_itemsSource is IDeclarativeProperty<IEnumerable<TItem>> enumerableDeclarativeProperty)
                _getItems = () => enumerableDeclarativeProperty.Value;
            else if (_itemsSource is ICollection<TItem> collection)
                _getItems = () => collection;
            else if (_itemsSource is TItem[] array)
                _getItems = () => array;
            else if (_itemsSource is IEnumerable<TItem> enumerable)
                _getItems = () => enumerable.ToArray();

            if (_listViewItems is not null)
            {
                ListViewItemPool.Return(_listViewItems);
                _listViewItems = null;
            }

            OnPropertyChanged();
        }
    }

    public override void Render()
    {
        if (_getItems is null) return;
        var items = _getItems().ToList();

        Span<ListViewItem<TItem>> listViewItems = null;

        if (_listViewItems is null || _listViewItems.Length != items.Count)
        {
            var newListViewItems = ListViewItemPool.Rent(items.Count);
            for (var i = 0; i < items.Count; i++)
            {
                var dataContext = items[i];
                newListViewItems[i] = CreateChild<ListViewItem<TItem>, TItem>(_ => dataContext);
            }

            _listViewItems = newListViewItems;
            _listViewItemLength = items.Count;
            listViewItems = newListViewItems[..items.Count];
        }
        else
        {
            listViewItems = _listViewItems[.._listViewItemLength];
        }

        foreach (var item in listViewItems)
        {
            item.Render();
        }
    }
}