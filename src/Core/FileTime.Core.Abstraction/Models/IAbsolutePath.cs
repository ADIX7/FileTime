using FileTime.Core.Enums;
using FileTime.Core.Services;

namespace FileTime.Core.Models
{
    public interface IAbsolutePath
    {
        IContentProvider ContentProvider { get; }
        IContentProvider? VirtualContentProvider { get; }
        FullName Path { get; }
        AbsolutePathType Type { get; }
    }
}