using System.Buffers;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using DeclarativeProperty;
using TerminalUI.Models;

namespace TerminalUI.Controls;

public class ListView<TDataContext, TItem> : View<TDataContext>
{
    private static readonly ArrayPool<ListViewItem<TItem>> ListViewItemPool = ArrayPool<ListViewItem<TItem>>.Shared;

    private readonly List<IDisposable> _itemsDisposables = new();
    private Func<IEnumerable<TItem>?>? _getItems;
    private object? _itemsSource;
    private ListViewItem<TItem>[]? _listViewItems;
    private int _listViewItemLength;
    private int _selectedIndex = 0;
    private int _renderStartIndex = 0;
    private Size _requestedItemSize = new(0, 0);

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex != value)
            {
                _selectedIndex = value;
                OnPropertyChanged();
                ApplicationContext?.EventLoop.RequestRerender();
            }
        }
    }

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
            {
                observableDeclarativeProperty.PropertyChanged += (_, _) => ApplicationContext?.EventLoop.RequestRerender();
                _getItems = () => observableDeclarativeProperty.Value;
            }
            else if (_itemsSource is IDeclarativeProperty<ReadOnlyObservableCollection<TItem>> readOnlyObservableDeclarativeProperty)
            {
                readOnlyObservableDeclarativeProperty.PropertyChanged += (_, _) => ApplicationContext?.EventLoop.RequestRerender();
                _getItems = () => readOnlyObservableDeclarativeProperty.Value;
            }
            else if (_itemsSource is IDeclarativeProperty<IEnumerable<TItem>> enumerableDeclarativeProperty)
            {
                enumerableDeclarativeProperty.PropertyChanged += (_, _) => ApplicationContext?.EventLoop.RequestRerender();
                _getItems = () => enumerableDeclarativeProperty.Value;
            }
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

    public Func<ListViewItem<TItem>, IView?> ItemTemplate { get; set; } = DefaultItemTemplate;

    public override Size GetRequestedSize()
    {
        if (_listViewItems is null || _listViewItems.Length == 0)
            return new Size(0, 0);


        var itemSize = _listViewItems[0].GetRequestedSize();
        _requestedItemSize = itemSize;
        return itemSize with {Height = itemSize.Height * _listViewItems.Length};
    }

    protected override void DefaultRenderer(Position position, Size size)
    {
        var listViewItems = InstantiateItemViews();
        if (listViewItems.Length == 0) return;

        var requestedItemSize = _requestedItemSize;

        var itemsToRender = listViewItems.Length;
        var heightNeeded = requestedItemSize.Height * listViewItems.Length;
        var renderStartIndex = _renderStartIndex;
        if (heightNeeded < size.Height)
        {
            var maxItemsToRender = (int) Math.Floor((double) size.Height / requestedItemSize.Height);
            if (SelectedIndex < renderStartIndex)
            {
                renderStartIndex = SelectedIndex - 1;
            }
            else if (SelectedIndex > renderStartIndex + maxItemsToRender)
            {
                renderStartIndex = SelectedIndex - maxItemsToRender + 1;
            }
            
            if(renderStartIndex < 0)
                renderStartIndex = 0;
            else if (renderStartIndex + maxItemsToRender > listViewItems.Length)
                renderStartIndex = listViewItems.Length - maxItemsToRender;

            _renderStartIndex = renderStartIndex;
        }

        var deltaY = 0;
        for (var i = renderStartIndex; i < itemsToRender && i < listViewItems.Length; i++)
        {
            var item = listViewItems[i];
            item.Render(position with {Y = position.Y + deltaY}, requestedItemSize);
            deltaY += requestedItemSize.Height;
        }
    }

    private Span<ListViewItem<TItem>> InstantiateItemViews()
    {
        var items = _getItems?.Invoke()?.ToList();
        if (items is null)
        {
            if (_listViewItemLength != 0)
            {
                return InstantiateEmptyItemViews();
            }

            return _listViewItems;
        }

        Span<ListViewItem<TItem>> listViewItems;

        if (_listViewItems is null || _listViewItemLength != items.Count)
        {
            var newListViewItems = ListViewItemPool.Rent(items.Count);
            for (var i = 0; i < items.Count; i++)
            {
                var dataContext = items[i];
                var child = CreateChild<ListViewItem<TItem>, TItem>(_ => dataContext);
                child.Content = ItemTemplate(child);
                ItemTemplate(child);
                newListViewItems[i] = child;
            }

            _listViewItems = newListViewItems;
            _listViewItemLength = items.Count;
            listViewItems = newListViewItems[..items.Count];
        }
        else
        {
            listViewItems = _listViewItems[.._listViewItemLength];
        }

        return listViewItems;
    }

    private Span<ListViewItem<TItem>> InstantiateEmptyItemViews()
    {
        _listViewItems = ListViewItemPool.Rent(0);
        _listViewItemLength = 0;
        return _listViewItems;
    }

    private static IView? DefaultItemTemplate(ListViewItem<TItem> listViewItem) => null;
}