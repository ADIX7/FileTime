using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ObservableComputations;

namespace DeclarativeProperty;

public sealed class ExtractorProperty<T, TResult> : DeclarativePropertyBase<TResult>
{
    private interface ICollectionWrapper : IDisposable
    {
        IList<T> Collection { get; }
    }

    private interface ICollectionWrapper<in TCollection> : ICollectionWrapper
        where TCollection : IList<T>, INotifyCollectionChanged
    {
        static abstract ICollectionWrapper<TCollection> Create(
            TCollection collection,
            Action<object?, NotifyCollectionChangedEventArgs> handler
        );
    }

    private class ObservableCollectionWrapper : ICollectionWrapper<ObservableCollection<T>>
    {
        private readonly NotifyCollectionChangedEventHandler _handler;
        private readonly ObservableCollection<T> _collection;

        public ObservableCollectionWrapper(
            ObservableCollection<T> collection,
            Action<object?, NotifyCollectionChangedEventArgs> handler
        )
        {
            _collection = collection;
            _handler = new NotifyCollectionChangedEventHandler(handler);

            _collection.CollectionChanged += _handler;
        }

        public IList<T> Collection => _collection;

        public void Dispose() => _collection.CollectionChanged -= _handler;


        public static ICollectionWrapper<ObservableCollection<T>> Create(
            ObservableCollection<T> collection,
            Action<object?, NotifyCollectionChangedEventArgs> handler
        ) => new ObservableCollectionWrapper(collection, handler);
    }

    private class ReadOnlyObservableCollectionWrapper : ICollectionWrapper<ReadOnlyObservableCollection<T>>
    {
        private readonly NotifyCollectionChangedEventHandler _handler;
        private readonly ReadOnlyObservableCollection<T> _collection;

        public ReadOnlyObservableCollectionWrapper(
            ReadOnlyObservableCollection<T> collection,
            Action<object?, NotifyCollectionChangedEventArgs> handler
        )
        {
            _collection = collection;
            _handler = new NotifyCollectionChangedEventHandler(handler);

            ((INotifyCollectionChanged) _collection).CollectionChanged += _handler;
        }

        public IList<T> Collection => _collection;

        public void Dispose() => ((INotifyCollectionChanged) _collection).CollectionChanged -= _handler;


        public static ICollectionWrapper<ReadOnlyObservableCollection<T>> Create(
            ReadOnlyObservableCollection<T> collection,
            Action<object?, NotifyCollectionChangedEventArgs> handler
        ) => new ReadOnlyObservableCollectionWrapper(collection, handler);
    }

    private class CollectionComputingWrapper : ICollectionWrapper<CollectionComputing<T>>
    {
        private readonly OcConsumer _consumer;
        private readonly NotifyCollectionChangedEventHandler _handler;
        private readonly CollectionComputing<T> _collection;

        public CollectionComputingWrapper(
            CollectionComputing<T> collection,
            Action<object?, NotifyCollectionChangedEventArgs> handler
        )
        {
            _collection = collection;
            _handler = new NotifyCollectionChangedEventHandler(handler);

            ((INotifyCollectionChanged) _collection).CollectionChanged += _handler;

            collection.For(_consumer = new OcConsumer());
        }

        public IList<T> Collection => _collection;

        public void Dispose()
        {
            ((INotifyCollectionChanged) _collection).CollectionChanged -= _handler;
            _consumer.Dispose();
        }


        public static ICollectionWrapper<CollectionComputing<T>> Create(
            CollectionComputing<T> collection,
            Action<object?, NotifyCollectionChangedEventArgs> handler
        ) => new CollectionComputingWrapper(collection, handler);
    }

    private readonly Func<IList<T>?, TResult> _extractor;
    private ICollectionWrapper? _collectionWrapper;

    public ExtractorProperty(
        IDeclarativeProperty<ObservableCollection<T>?> from,
        Func<IList<T>?, TResult> extractor) : base(extractor(from.Value))
    {
        _extractor = extractor;
        _collectionWrapper = from.Value is null
            ? null
            : new ObservableCollectionWrapper(from.Value, CollectionUpdated);

        AddDisposable(from.Subscribe(SetValue<ObservableCollectionWrapper, ObservableCollection<T>>));
    }

    public ExtractorProperty(
        IDeclarativeProperty<ReadOnlyObservableCollection<T>?> from,
        Func<IList<T>?, TResult> extractor) : base(extractor(from.Value))
    {
        _extractor = extractor;
        _collectionWrapper = from.Value is null
            ? null
            : new ReadOnlyObservableCollectionWrapper(from.Value, CollectionUpdated);

        AddDisposable(from.Subscribe(SetValue<ReadOnlyObservableCollectionWrapper, ReadOnlyObservableCollection<T>>));
    }

    public ExtractorProperty(
        IDeclarativeProperty<CollectionComputing<T>?> from,
        Func<IList<T>?, TResult> extractor) : base(extractor(from.Value))
    {
        _extractor = extractor;
        _collectionWrapper = from.Value is null
            ? null
            : new CollectionComputingWrapper(from.Value, CollectionUpdated);

        AddDisposable(from.Subscribe(SetValue<CollectionComputingWrapper, CollectionComputing<T>>));
    }

    private void CollectionUpdated(object? sender, NotifyCollectionChangedEventArgs e)
        => Task.Run(async () => await FireAsync(_collectionWrapper?.Collection)).Wait();

    private Task SetValue<TWrapper, TCollection>(TCollection? next, CancellationToken cancellationToken = default)
        where TCollection : IList<T>, INotifyCollectionChanged
        where TWrapper : ICollectionWrapper<TCollection>
    {
        _collectionWrapper?.Dispose();

        _collectionWrapper = next is null
            ? null
            : TWrapper.Create(next, CollectionUpdated);

        return FireAsync(next, cancellationToken);
    }

    private Task FireAsync(IList<T>? items, CancellationToken cancellationToken = default)
    {
        var newValue = _extractor(items);

        return SetNewValueAsync(newValue, cancellationToken);
    }
}