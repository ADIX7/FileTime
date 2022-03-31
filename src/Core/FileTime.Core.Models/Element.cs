using FileTime.Core.Enums;
using FileTime.Core.Services;

namespace FileTime.Core.Models
{
    public record Element
        (string Name,
        string DisplayName,
        FullName FullName,
        NativePath NativePath,
        bool IsHidden,
        bool IsExists,
        SupportsDelete CanDelete,
        bool CanRename,
        IContentProvider Provider) : IElement;
}