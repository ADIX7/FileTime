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

    public ItemNotFoundException(string path) : base("Item not found " + path)
    {
        Path = path;
    }

    public ItemNotFoundException(FullName path) : this(path.Path)
    {
        Type = ItemNotFoundExceptionType.FullName;
    }

    public ItemNotFoundException(NativePath path) : this(path.Path)
    {
        Type = ItemNotFoundExceptionType.NativePath;
    }
}