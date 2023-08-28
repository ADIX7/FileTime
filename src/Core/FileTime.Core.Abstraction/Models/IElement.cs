namespace FileTime.Core.Models;

public interface IElement : IItem
{
    long Size { get; }
}