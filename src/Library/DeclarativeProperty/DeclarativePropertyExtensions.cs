﻿using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ObservableComputations;

namespace DeclarativeProperty;

public static class DeclarativePropertyExtensions
{
    public static IDeclarativeProperty<T> Debounce<T>(this IDeclarativeProperty<T> from, TimeSpan interval, bool resetTimer = false)
        => new DebounceProperty<T>(from, interval){ResetTimer = resetTimer};

    public static IDeclarativeProperty<T> DistinctUntilChanged<T>(this IDeclarativeProperty<T> from)
        => new DistinctUntilChangedProperty<T>(from);

    public static IDeclarativeProperty<TTo?> Map<TFrom, TTo>(this IDeclarativeProperty<TFrom?> from, Func<TFrom?, CancellationToken, Task<TTo?>> mapper)
        => new MapProperty<TFrom?, TTo?>(mapper, from);

    public static IDeclarativeProperty<TTo?> Map<TFrom, TTo>(this IDeclarativeProperty<TFrom?> from, Func<TFrom?, TTo?> mapper)
        => new MapProperty<TFrom?, TTo?>((next, _) => Task.FromResult(mapper(next)), from);

    public static async Task<IDeclarativeProperty<TTo?>> MapAsync<TFrom, TTo>(this IDeclarativeProperty<TFrom?> from, Func<TFrom?, CancellationToken, Task<TTo?>> mapper)
        => await MapProperty<TFrom?, TTo?>.CreateAsync(mapper, from);

    public static async Task<IDeclarativeProperty<TTo?>> MapAsync<TFrom, TTo>(this IDeclarativeProperty<TFrom?> from, Func<TFrom?, TTo?> mapper)
        => await MapProperty<TFrom?, TTo?>.CreateAsync((next, _) => Task.FromResult(mapper(next)), from);

    public static IDisposable Subscribe<T>(this IDeclarativeProperty<T> property, Action<T?, CancellationToken> onChange)
        => property.Subscribe((value, token) =>
        {
            onChange(value, token);
            return Task.CompletedTask;
        });

    public static IDisposable Subscribe<T>(this IDeclarativeProperty<T> property, Action<T?> onChange)
        => property.Subscribe((value, _) =>
        {
            onChange(value);
            return Task.CompletedTask;
        });

    public static IDeclarativeProperty<T> Extract<T>(
        this IDeclarativeProperty<ReadOnlyObservableCollection<T>> from,
        Func<IList<T>?, T?> extractor
    )
        => new ExtractorProperty<T>(from, extractor);

    public static IDeclarativeProperty<T> Extract<T>(
        this IDeclarativeProperty<ObservableCollection<T>> from,
        Func<IList<T>?, T?> extractor
    )
        => new ExtractorProperty<T>(from, extractor);

    public static IDeclarativeProperty<T> Extract<T>(
        this IDeclarativeProperty<CollectionComputing<T>> from,
        Func<IList<T>?, T?> extractor
    )
        => new ExtractorProperty<T>(from, extractor);

    public static IDeclarativeProperty<TCollection?> Watch<TCollection, TItem>(
        this IDeclarativeProperty<TCollection?> collection)
        where TCollection : IList<TItem>, INotifyCollectionChanged
        => new CollectionRepeaterProperty<TCollection?, TItem>(collection);
}