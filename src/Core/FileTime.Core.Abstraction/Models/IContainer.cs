using System.Collections.ObjectModel;
using DynamicData;

namespace FileTime.Core.Models;

public interface IContainer : IItem
{
    IObservable<IChangeSet<AbsolutePath, string>> Items { get; }
    ReadOnlyObservableCollection<AbsolutePath> ItemsCollection { get; }
    IObservable<bool> IsLoading { get; }
    bool? IsLoaded { get; }
    Task WaitForLoaded(CancellationToken token = default);
    bool AllowRecursiveDeletion { get; }
}