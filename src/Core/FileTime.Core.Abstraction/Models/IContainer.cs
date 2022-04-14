namespace FileTime.Core.Models
{
    public interface IContainer : IItem
    {
        IObservable<IEnumerable<IAbsolutePath>?> Items { get; }
        IObservable<bool> IsLoading { get; }
    }
}