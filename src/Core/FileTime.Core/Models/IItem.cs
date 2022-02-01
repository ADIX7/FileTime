using FileTime.Core.Providers;

namespace FileTime.Core.Models
{
    public interface IItem
    {
        string Name { get; }
        string? FullName { get; }
        bool IsHidden { get; }
        bool CanDelete { get; }
        bool CanRename { get; }
        IContentProvider Provider { get; }
        Task Delete();
        Task Rename(string newName);
        IContainer? GetParent();
    }
}