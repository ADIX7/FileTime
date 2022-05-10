using System.Collections.ObjectModel;
using System.ComponentModel;
using DynamicData;
using PropertyChanged.SourceGenerator;

namespace FileTime.App.Core.Models;

public partial class BindedCollection<T> : IDisposable, INotifyPropertyChanged
{
    private readonly IDisposable _disposable;
    private IDisposable? _innerDisposable;

    [Notify] private ReadOnlyObservableCollection<T>? _collection;

    public BindedCollection(IObservable<IChangeSet<T>> dynamicList)
    {
        _disposable = dynamicList
            .Bind(out var collection)
            .DisposeMany()
            .Subscribe();

        _collection = collection;
    }

    public BindedCollection(IObservable<IObservable<IChangeSet<T>>?> dynamicListSource)
    {
        _disposable = dynamicListSource.Subscribe(dynamicList =>
        {
            _innerDisposable?.Dispose();
            if (dynamicList is not null)
            {
                _innerDisposable = dynamicList
                    .Bind(out var collection)
                    .DisposeMany()
                    .Subscribe();

                Collection = collection;
            }
            else
            {
                Collection = null;
            }
        });
    }

    public void Dispose()
    {
        _disposable.Dispose();
        _innerDisposable?.Dispose();
        GC.SuppressFinalize(this);
    }
}