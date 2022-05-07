using System.Reactive.Linq;

namespace FileTime.App.Core.Extensions;

public static class ObservableExtensions
{
    public static IObservable<T> WhereNotNull<T>(this IObservable<T?> source) => source.Where(c => c != null)!;
}