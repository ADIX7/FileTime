using DynamicData;
using FileTime.Core.Models;

namespace FileTime.Core.Extensions;

public static class BindedCollectionExtensions
{
    public static BindedCollection<T> ToBindedCollection<T>(this IObservable<IChangeSet<T>> source)
    {
        return new BindedCollection<T>(source);
    }

    public static BindedCollection<T> ToBindedCollection<T>(this IObservable<IObservable<IChangeSet<T>>?> source)
    {
        return new BindedCollection<T>(source);
    }

    public static BindedCollection<T, TKey> ToBindedCollection<T, TKey>(this IObservable<IChangeSet<T, TKey>> source) where TKey : notnull
    {
        return new BindedCollection<T, TKey>(source);
    }

    public static BindedCollection<T, TKey> ToBindedCollection<T, TKey>(this IObservable<IObservable<IChangeSet<T, TKey>>?> source) where TKey : notnull
    {
        return new BindedCollection<T, TKey>(source);
    }
}