namespace FileTime.Core.Models
{
    public interface IContainer : IItem
    {
        IObservable<IReadOnlyList<IAbsolutePath>> Items { get; }
        IObservable<bool> IsLoading { get; }
    }
}