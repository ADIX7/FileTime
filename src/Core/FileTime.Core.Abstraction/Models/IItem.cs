using FileTime.Core.Enums;
using FileTime.Core.Services;

namespace FileTime.Core.Models
{
    public interface IItem
    {
        string Name { get; }
        string DisplayName { get; }
        FullName? FullName { get; }
        NativePath? NativePath { get; }
        IAbsolutePath? Parent { get; }
        bool IsHidden { get; }
        bool IsExists { get; }
        DateTime? CreatedAt { get; }
        SupportsDelete CanDelete { get; }
        bool CanRename { get; }
        IContentProvider Provider { get; }
        string? Attributes { get; }
    }
}