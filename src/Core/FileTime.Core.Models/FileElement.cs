using FileTime.Core.Enums;
using FileTime.Core.Services;

namespace FileTime.Core.Models
{
    public record FileElement(
        string Name,
        string DisplayName,
        FullName FullName,
        NativePath NativePath,
        IAbsolutePath? Parent,
        bool IsHidden,
        bool IsExists,
        DateTime? CreatedAt,
        SupportsDelete CanDelete,
        bool CanRename,
        string? Attributes,
        IContentProvider Provider,
        IObservable<IEnumerable<Exception>> Exceptions,
        long Size)
    : Element(
        Name,
        DisplayName,
        FullName,
        NativePath,
        Parent,
        IsHidden,
        IsExists,
        CreatedAt,
        CanDelete,
        CanRename,
        Attributes,
        Provider,
        Exceptions
    ), IFileElement;
}