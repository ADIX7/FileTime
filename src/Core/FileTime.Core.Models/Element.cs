using FileTime.Core.Enums;
using FileTime.Core.Services;

namespace FileTime.Core.Models
{
    public record Element(
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
        IContentProvider Provider) : IElement;
}