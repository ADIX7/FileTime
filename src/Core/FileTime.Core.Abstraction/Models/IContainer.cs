using DynamicData;

namespace FileTime.Core.Models;

public interface IContainer : IItem
{
    IObservable<IObservable<IChangeSet<IAbsolutePath>>?> Items { get; }
    IObservable<bool> IsLoading { get; }
}