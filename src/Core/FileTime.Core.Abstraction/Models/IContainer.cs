namespace FileTime.Core.Models
{
    public interface IContainer : IItem
    {
        IReadOnlyList<IAbsolutePath> Items { get; }
    }
}