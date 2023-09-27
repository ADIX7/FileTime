using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ObservableComputations;

namespace DeclarativeProperty;

public static class DeclarativePropertyExtensions
{
    public static IDeclarativeProperty<T> Debounce<T>(this IDeclarativeProperty<T> from, TimeSpan interval, bool resetTimer = false)
        => new DebounceProperty<T>(from, _ => interval) {ResetTimer = resetTimer};

    public static IDeclarativeProperty<T> Debounce<T>(this IDeclarativeProperty<T> from, Func<T?, TimeSpan> interval, bool resetTimer = false)
        => new DebounceProperty<T>(from, interval) {ResetTimer = resetTimer};

    public static IDeclarativeProperty<T> Throttle<T>(this IDeclarativeProperty<T> from, TimeSpan interval)
        => new ThrottleProperty<T>(from, () => interval);

    public static IDeclarativeProperty<T> Throttle<T>(this IDeclarativeProperty<T> from, Func<TimeSpan> interval)
        => new ThrottleProperty<T>(from, interval);

    public static IDeclarativeProperty<T> DistinctUntilChanged<T>(this IDeclarativeProperty<T> from)
        => new DistinctUntilChangedProperty<T>(from);

    public static IDeclarativeProperty<TTo> Map<TFrom, TTo>(this IDeclarativeProperty<TFrom> from, Func<TFrom, Task<TTo>> mapper)
        => Map(from, async (v, _) => await mapper(v));

    public static IDeclarativeProperty<TTo> Map<TFrom, TTo>(this IDeclarativeProperty<TFrom> from, Func<TFrom, CancellationToken, Task<TTo>> mapper)
        => new MapProperty<TFrom, TTo>(mapper, from);

    public static IDeclarativeProperty<TTo> Map<TFrom, TTo>(this IDeclarativeProperty<TFrom> from, Func<TFrom, TTo> mapper)
        => new MapProperty<TFrom, TTo>((next, _) => Task.FromResult(mapper(next)), from);

    public static async Task<IDeclarativeProperty<TTo>> MapAsync<TFrom, TTo>(this IDeclarativeProperty<TFrom> from, Func<TFrom, CancellationToken, Task<TTo>> mapper)
        => await MapProperty<TFrom, TTo>.CreateAsync(mapper, from);

    public static async Task<IDeclarativeProperty<TTo?>> MapAsync<TFrom, TTo>(this IDeclarativeProperty<TFrom?> from, Func<TFrom?, TTo?> mapper)
        => await MapProperty<TFrom?, TTo?>.CreateAsync((next, _) => Task.FromResult(mapper(next)), from);

    public static IDisposable Subscribe<T>(this IDeclarativeProperty<T> property, Action<T?, CancellationToken> onChange)
        => property.Subscribe((value, token) =>
        {
            onChange(value, token);
            return Task.CompletedTask;
        });

    public static IDisposable Subscribe<T>(this IDeclarativeProperty<T> property, Action<T> onChange)
        => property.Subscribe((value, _) =>
        {
            onChange(value);
            return Task.CompletedTask;
        });

    public static IDeclarativeProperty<TResult> Extract<T, TResult>(
        this IDeclarativeProperty<ReadOnlyObservableCollection<T>?> from,
        Func<IList<T>?, TResult> extractor
    )
        => new ExtractorProperty<T, TResult>(from, extractor);

    public static IDeclarativeProperty<TResult> Extract<T, TResult>(
        this IDeclarativeProperty<ObservableCollection<T>?> from,
        Func<IList<T>?, TResult> extractor
    )
        => new ExtractorProperty<T, TResult>(from, extractor);

    public static IDeclarativeProperty<TResult> Extract<T, TResult>(
        this IDeclarativeProperty<CollectionComputing<T>?> from,
        Func<IList<T>?, TResult> extractor
    )
        => new ExtractorProperty<T, TResult>(from, extractor);

    public static IDeclarativeProperty<TCollection> Watch<TCollection, TItem>(
        this IDeclarativeProperty<TCollection> collection)
        where TCollection : IList<TItem>?, INotifyCollectionChanged?
        => new CollectionRepeaterProperty<TCollection, TItem>(collection);

    public static IDeclarativeProperty<TCollection> Watch<TCollection, TItem>(
        this TCollection collection)
        where TCollection : IList<TItem>?, INotifyCollectionChanged?
        => new CollectionRepeaterProperty<TCollection, TItem>(collection);

    public static IDeclarativeProperty<ObservableCollection<TItem>> Watch<TItem>(
        this ObservableCollection<TItem> collection)
        => new CollectionRepeaterProperty<ObservableCollection<TItem>, TItem>(collection);

    public static IDeclarativeProperty<ReadOnlyObservableCollection<TItem>> Watch<TItem>(
        this ReadOnlyObservableCollection<TItem> collection)
        => new CollectionRepeaterProperty<ReadOnlyObservableCollection<TItem>, TItem>(collection);
    
    
    public static IDeclarativeProperty<TResult> CombineLatest<T1, T2, TResult>(
        this IDeclarativeProperty<T1> prop1,
        IDeclarativeProperty<T2> prop2,
        Func<T1, T2, Task<TResult>> func,
        Action<TResult?>? setValueHook = null)
        => new CombineLatestProperty<T1,T2,TResult>(prop1, prop2, func, setValueHook);

    public static IDeclarativeProperty<T> Switch<T>(this IDeclarativeProperty<IDeclarativeProperty<T>?> from)
        => new SwitchProperty<T>(from);
    
    public static IDeclarativeProperty<TResult> CombineAll<T, TResult>(
        this IEnumerable<IDeclarativeProperty<T>> sources,
        Func<IEnumerable<T>, Task<TResult>> combiner,
        Action<TResult?>? setValueHook = null)
        => new CombineAllProperty<T,TResult>(sources, combiner, setValueHook);
}