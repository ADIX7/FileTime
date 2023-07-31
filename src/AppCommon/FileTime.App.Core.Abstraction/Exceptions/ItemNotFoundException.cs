using FileTime.Core.Models;

namespace FileTime.App.Core.Exceptions;

public enum ItemNotFoundExceptionType
{
    Raw,
    FullName,
    NativePath
}
public class ItemNotFoundException : Exception
{
    public string Path { get; }
    public ItemNotFoundExceptionType Type { get; } = ItemNotFoundExceptionType.Raw;

    public ItemNotFoundException(string path)
    {
        Path = path;
    }

    public ItemNotFoundException(FullName path)
    {
        Path = path.Path;
        Type = ItemNotFoundExceptionType.FullName;
    }

    public ItemNotFoundException(NativePath path)
    {
        Path = path.Path;
        Type = ItemNotFoundExceptionType.NativePath;
    }
}