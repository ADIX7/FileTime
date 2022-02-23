namespace FileTime.Core.Models
{
    public interface IItemWithSize : IItem
    {
        long? Size { get; }
    }
}