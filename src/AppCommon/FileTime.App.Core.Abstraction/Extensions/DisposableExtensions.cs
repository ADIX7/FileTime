namespace FileTime.App.Core.Extensions;

public static class DisposableExtensions
{
    public static void AddToDisposables(this IDisposable disposable, ICollection<IDisposable> collection)
        => collection.Add(disposable);
}