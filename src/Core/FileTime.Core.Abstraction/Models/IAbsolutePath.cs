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

        Task<IItem> ResolveAsync(bool forceResolve = false);
        Task<IItem?> ResolveAsyncSafe(bool forceResolve = false);
    }
}