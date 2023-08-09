﻿using System.Buffers;
using System.Collections.ObjectModel;
using DeclarativeProperty;
using PropertyChanged.SourceGenerator;
using TerminalUI.Models;

namespace TerminalUI.Controls;

public partial class ListView<TDataContext, TItem> : View<TDataContext>
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
    [Notify] private int _listPadding = 0;
    [Notify] private Orientation _orientation = Orientation.Vertical;

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex != value)
            {
                _selectedIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedItem));
            }
        }
    }

    public TItem? SelectedItem
    {
        get => _listViewItems is null ? default : _listViewItems[_selectedIndex].DataContext;
        set
        {
            if (_listViewItems is null || value is null) return;

            var newSelectedIndex = -1;
            for (var i = 0; i < _listViewItemLength; i++)
            {
                var dataContext = _listViewItems[i].DataContext;
                if (dataContext is null) continue;

                if (dataContext.Equals(value))
                {
                    newSelectedIndex = i;
                    break;
                }
            }

            if (newSelectedIndex != -1)
            {
                SelectedIndex = newSelectedIndex;
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

            _renderStartIndex = 0;
            SelectedIndex = 0;
            OnPropertyChanged();
        }
    }

    public Func<ListViewItem<TItem>, IView?> ItemTemplate { get; set; } = DefaultItemTemplate;

    public ListView()
    {
        RerenderProperties.Add(nameof(ItemsSource));
        RerenderProperties.Add(nameof(SelectedIndex));
        RerenderProperties.Add(nameof(Orientation));
    }

    public override Size GetRequestedSize()
    {
        InstantiateItemViews();
        if (_listViewItems is null || _listViewItemLength == 0)
            return new Size(0, 0);

        if (Orientation == Orientation.Vertical)
        {
            var itemSize = _listViewItems[0].GetRequestedSize();
            _requestedItemSize = itemSize;
            return itemSize with {Height = itemSize.Height * _listViewItemLength};
        }
        else
        {
            var width = 0;
            var height = 0;
            for (var i = 0; i < _listViewItemLength; i++)
            {
                var item = _listViewItems[i];
                width += item.GetRequestedSize().Width;
                height = Math.Max(height, item.GetRequestedSize().Height);
            }

            return new Size(width, height);
        }
    }

    protected override void DefaultRenderer(Position position, Size size)
    {
        if (Orientation == Orientation.Vertical)
            RenderVertical(position, size);
        else
            RenderHorizontal(position, size);
    }

    private void RenderHorizontal(Position position, Size size)
    {
        //Note: no support for same width elements
        var listViewItems = InstantiateItemViews();
        if (listViewItems.Length == 0) return;

        Span<Size> requestedSizes = stackalloc Size[_listViewItemLength];

        var totalRequestedWidth = 0;
        Span<int> widthSumUpToIndex = stackalloc int[_listViewItemLength];
        for (var i = 0; i < listViewItems.Length; i++)
        {
            widthSumUpToIndex[i] = totalRequestedWidth;
            var item = listViewItems[i];
            var requestedItemSize = item.GetRequestedSize();
            totalRequestedWidth += requestedItemSize.Width;
            requestedSizes[i] = requestedItemSize;
        }

        var renderStartIndex = _renderStartIndex;
        var lastItemIndex = _listViewItemLength;

        if (totalRequestedWidth > size.Width)
        {
            //Moving the render "window" to the right
            //Until the selected item's end is in it
            //So when RenderStartPosition (ie all the widths up to RenderStartIndex) + size.Width >= SelectedItemEnd
            var selectedIndexEnd = widthSumUpToIndex[SelectedIndex] + requestedSizes[SelectedIndex].Width;
            var startXOfRenderStartItem = widthSumUpToIndex[renderStartIndex];

            while (selectedIndexEnd > startXOfRenderStartItem + size.Width)
            {
                startXOfRenderStartItem += requestedSizes[renderStartIndex].Width;
                renderStartIndex++;
            }

            //Moving the render "window" to the left
            //Until the selected item's start is in it
            //So when RenderStartPosition (ie all the widths up to RenderStartIndex) <= SelectedItemStart
            var selectedIndexStart = widthSumUpToIndex[SelectedIndex];
            startXOfRenderStartItem = widthSumUpToIndex[renderStartIndex];
            while (selectedIndexStart < startXOfRenderStartItem)
            {
                renderStartIndex--;
                startXOfRenderStartItem -= requestedSizes[renderStartIndex].Width;
            }
        }

        var deltaX = 0;
        for (var i = renderStartIndex; i < _listViewItemLength; i++)
        {
            var item = listViewItems[i];
            var requestedItemSize = requestedSizes[i];
            var width = requestedItemSize.Width;
            var nextDeltaX = deltaX + requestedItemSize.Width;
            if (nextDeltaX > size.Width)
            {
                width = size.Width - deltaX;
            }

            item.Render(position with {X = position.X + deltaX}, size with {Width = width});
            deltaX = nextDeltaX;
        }
    }

    private void RenderVertical(Position position, Size size)
    {
        //Note: only same height is supported
        var requestedItemSize = _requestedItemSize;
        if (requestedItemSize.Height == 0 || requestedItemSize.Width == 0)
            return;

        var listViewItems = InstantiateItemViews();
        if (listViewItems.Length == 0) return;

        var itemsToRender = listViewItems.Length;
        var heightNeeded = requestedItemSize.Height * listViewItems.Length;
        var renderStartIndex = _renderStartIndex;
        if (heightNeeded > size.Height)
        {
            var maxItemsToRender = (int) Math.Floor((double) size.Height / requestedItemSize.Height);
            itemsToRender = maxItemsToRender;

            if (SelectedIndex - ListPadding < renderStartIndex)
            {
                renderStartIndex = SelectedIndex - ListPadding;
            }
            else if (SelectedIndex + ListPadding >= renderStartIndex + maxItemsToRender)
            {
                renderStartIndex = SelectedIndex + ListPadding - maxItemsToRender + 1;
            }

            if (renderStartIndex + itemsToRender > listViewItems.Length)
                renderStartIndex = listViewItems.Length - itemsToRender;

            if (renderStartIndex < 0)
                renderStartIndex = 0;

            _renderStartIndex = renderStartIndex;
        }

        var deltaY = 0;
        var lastItemIndex = renderStartIndex + itemsToRender;
        if (lastItemIndex > listViewItems.Length)
            lastItemIndex = listViewItems.Length;

        for (var i = renderStartIndex; i < lastItemIndex; i++)
        {
            var item = listViewItems[i];
            item.Render(position with {Y = position.Y + deltaY}, requestedItemSize with {Width = size.Width});
            deltaY += requestedItemSize.Height;
        }

        var driver = ApplicationContext!.ConsoleDriver;
        var placeholder = new string(' ', size.Width);
        driver.ResetColor();
        for (var i = deltaY; i < size.Height; i++)
        {
            driver.SetCursorPosition(position with {Y = position.Y + i});
            driver.Write(placeholder);
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