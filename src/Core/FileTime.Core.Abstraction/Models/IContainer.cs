using DynamicData;

namespace FileTime.Core.Models;

public interface IContainer : IItem
{
    IObservable<IObservable<IChangeSet<AbsolutePath>>?> Items { get; }
    IObservable<bool> IsLoading { get; }
}