namespace FileTime.Core.Models;

public interface IFileElement : IElement
{
    long Size { get; }
}