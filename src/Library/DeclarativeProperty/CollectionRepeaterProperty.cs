using System.Collections.Specialized;

namespace DeclarativeProperty;

public class CollectionRepeaterProperty<TCollection, TItem> : DeclarativePropertyBase<TCollection>
    where TCollection : IList<TItem>, INotifyCollectionChanged
{
    private TCollection? _currentCollection;

    public CollectionRepeaterProperty(IDeclarativeProperty<TCollection?> from)
    {
        _currentCollection = from.Value;
        if (from.Value is { } value)
        {
            value.CollectionChanged += HandleCollectionChanged;
        }
        AddDisposable(from.Subscribe(Handle));
    }

    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var t = Task.Run(async () => await NotifySubscribersAsync(Value));
        t.Wait();
    }

    private async Task Handle(TCollection? collection, CancellationToken cancellationToken = default)
    {
        if (_currentCollection is { } currentCollection)
        {
            currentCollection.CollectionChanged -= HandleCollectionChanged;
        }
        if (collection is {} newCollection)
        {
            //newCollection.CollectionChanged -= HandleCollectionChanged;
            newCollection.CollectionChanged += HandleCollectionChanged;
        }

        _currentCollection = collection;

        await SetNewValueAsync(collection, cancellationToken);
    }
}