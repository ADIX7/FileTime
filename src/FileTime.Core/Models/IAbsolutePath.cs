using FileTime.Core.Providers;

namespace FileTime.Core.Models
{
    public interface IAbsolutePath
    {
        IContentProvider ContentProvider { get; }
        string Path { get; }
    }
}