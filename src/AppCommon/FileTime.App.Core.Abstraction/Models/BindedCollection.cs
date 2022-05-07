using System.Collections.ObjectModel;
using DynamicData;

namespace FileTime.App.Core.Models;

public class BindedCollection<T> : IDisposable
{
    private readonly IDisposable _disposable;
    public ReadOnlyObservableCollection<T> Collection { get; }
    public BindedCollection(IObservable<IChangeSet<T>> dynamicList)
    {
        _disposable = dynamicList
            .Bind(out var collection)
            .DisposeMany()
            .Subscribe();

        Collection = collection;
    }

    public void Dispose()
    {
        _disposable.Dispose();
        GC.SuppressFinalize(this);
    }
}