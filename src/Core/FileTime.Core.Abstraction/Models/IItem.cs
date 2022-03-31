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
        FullName? Parent { get; }
        bool IsHidden { get; }
        bool IsExists { get; }
        SupportsDelete CanDelete { get; }
        bool CanRename { get; }
        IContentProvider Provider { get; }
    }
}