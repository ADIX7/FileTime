using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ObservableComputations;
using PropertyChanged.SourceGenerator;
using TerminalUI.Models;
using TerminalUI.Traits;

namespace TerminalUI.Controls;

public sealed partial class ItemsControl<TDataContext, TItem>
    : View<ItemsControl<TDataContext, TItem>, TDataContext>, IVisibilityChangeHandler
{
    private readonly List<IView> _forceRerenderChildren = new();
    private readonly object _forceRerenderChildrenLock = new();
    private readonly List<IDisposable> _itemsDisposables = new();
    private readonly Dictionary<IView, Size> _requestedSizes = new();
    private IList<IView<TItem>> _children = new List<IView<TItem>>();
    private object? _itemsSource;
    [Notify] private Orientation _orientation = Orientation.Vertical;

    public Func<IView<TItem>> ItemTemplate { get; set; } = DefaultItemTemplate;

    public IReadOnlyList<IView<TItem>> Children => _children.AsReadOnly();

    public object? ItemsSource
    {
        get => _itemsSource;
        set
        {
            if (_itemsSource == value) return;
            
            if(_itemsSource is INotifyCollectionChanged notifyCollectionChanged)
                notifyCollectionChanged.CollectionChanged -= SourceCollectionChanged;
            
            _itemsSource = value;

            foreach (var disposable in _itemsDisposables)
            {
                disposable.Dispose();
            }

            _itemsDisposables.Clear();

            if (_itemsSource is ObservableCollection<TItem> observableDeclarative)
            {
                var consumer = new OcConsumer();
                var children = observableDeclarative
                    .Selecting(i => CreateItem(i))
                    .For(consumer);
                
                children.CollectionChanged += SourceCollectionChanged;
                _children = children;
                
                _itemsDisposables.Add(consumer);
            }
            else if (_itemsSource is ReadOnlyObservableCollection<TItem> readOnlyObservableDeclarative)
            {
                var consumer = new OcConsumer();
                var children = readOnlyObservableDeclarative
                    .Selecting(i => CreateItem(i))
                    .For(consumer);
                
                children.CollectionChanged += SourceCollectionChanged;
                _children = children;
                
                _itemsDisposables.Add(consumer);
            }
            else if (_itemsSource is ICollection<TItem> collection)
                _children = collection.Select(CreateItem).ToList();
            else if (_itemsSource is TItem[] array)
                _children = array.Select(CreateItem).ToList();
            else if (_itemsSource is IEnumerable<TItem> enumerable)
                _children = enumerable.Select(CreateItem).ToList();
            else if (value is null)
            {
                _children = new List<IView<TItem>>();
            }
            else
            {
                throw new NotSupportedException();
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(Children));
        }
    }

    public ItemsControl()
    {
        RerenderProperties.Add(nameof(ItemsSource));
        RerenderProperties.Add(nameof(Orientation));
    }

    private void SourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RequestRerenderForThis();

    private void RequestRerenderForThis() 
        => ApplicationContext?.RenderEngine.RequestRerender(this);

    protected override Size CalculateSize()
    {
        _requestedSizes.Clear();
        double width = 0;
        double height = 0;

        var children = _children.ToList();
        foreach (var child in children)
        {
            if (!child.IsVisible) continue;

            var childSize = child.GetRequestedSize();
            _requestedSizes.Add(child, childSize);

            if (Orientation == Orientation.Vertical)
            {
                width = Math.Max(width, childSize.Width);
                height += childSize.Height;
            }
            else
            {
                width += childSize.Width;
                height = Math.Max(height, childSize.Height);
            }
        }

        return new Size((int) width, (int) height);
    }

    protected override bool DefaultRenderer(in RenderContext renderContext, Position position, Size size)
    {
        var neededRerender = false;
        IReadOnlyList<IView> forceRerenderChildren;
        IReadOnlyList<IView> children;
        lock (_forceRerenderChildrenLock)
        {
            forceRerenderChildren = _forceRerenderChildren.ToList();
            _forceRerenderChildren.Clear();
            children = _children.ToList();
        }

        var delta = 0;
        foreach (var child in children)
        {
            if (!child.IsVisible) continue;

            if (!_requestedSizes.TryGetValue(child, out var childSize)) continue;

            var childPosition = Orientation == Orientation.Vertical
                ? position with {Y = position.Y + delta}
                : position with {X = position.X + delta};

            childSize = Orientation == Orientation.Vertical
                ? childSize with {Width = size.Width}
                : childSize with {Height = size.Height};

            var endX = position.X + size.Width;
            var endY = position.Y + size.Height;

            if (childPosition.X > endX || childPosition.Y > endY) break;
            if (childPosition.X + childSize.Width > endX)
            {
                childSize = childSize with {Width = endX - childPosition.X};
            }

            if (childPosition.Y + childSize.Height > endY)
            {
                childSize = childSize with {Height = endY - childPosition.Y};
            }

            if (forceRerenderChildren.Contains(child))
            {
                var rerenderContext = renderContext with {ForceRerender = true};
                neededRerender = child.Render(rerenderContext, childPosition, childSize) || neededRerender;
            }
            else
            {
                neededRerender = child.Render(renderContext, childPosition, childSize) || neededRerender;
            }

            delta += Orientation == Orientation.Vertical
                ? childSize.Height
                : childSize.Width;
        }
        
        // TODO: clean non used space

        return neededRerender;
    }

    private IView<TItem> CreateItem(TItem dataContext)
    {
        var newItem = ItemTemplate();
        AddChild(newItem, _ => dataContext);
        return newItem;
    }

    private static IView<TItem> DefaultItemTemplate() => new TextBlock<TItem> {Text = typeof(TItem).ToString()};

    public void ChildVisibilityChanged(IView child)
    {
        var viewToForceRerender = child;
        while (viewToForceRerender.VisualParent != null && viewToForceRerender.VisualParent != this)
        {
            viewToForceRerender = viewToForceRerender.VisualParent;
        }

        if (viewToForceRerender.VisualParent != this) return;

        lock (_forceRerenderChildrenLock)
        {
            _forceRerenderChildren.Add(viewToForceRerender);
        }
    }
}