using DynamicData;

namespace FileTime.Core.Models;

public interface IContainer : IItem
{
    IObservable<IObservable<IChangeSet<AbsolutePath, string>>?> Items { get; }
    IObservable<bool> IsLoading { get; }
    bool AllowRecursiveDeletion { get; }
}