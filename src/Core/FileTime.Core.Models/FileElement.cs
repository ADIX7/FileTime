using FileTime.Core.Enums;
using FileTime.Core.Services;

namespace FileTime.Core.Models
{
    public record FileElement(
        string Name,
        string DisplayName,
        FullName FullName,
        NativePath NativePath,
        FullName Parent,
        bool IsHidden,
        bool IsExists,
        DateTime? CreatedAt,
        SupportsDelete CanDelete,
        bool CanRename,
        string? Attributes,
        IContentProvider Provider,
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
        Provider
    ), IFileElement;
}