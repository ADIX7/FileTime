using FileTime.Core.Providers;

namespace FileTime.Core.Models
{
    public interface IItem
    {
        string Name { get; }
        string? FullName { get; }
        string? NativePath { get; }
        bool IsHidden { get; }
        bool IsDestroyed { get; }
        SupportsDelete CanDelete { get; }
        bool CanRename { get; }
        IContentProvider Provider { get; }
        void Destroy();
        Task Delete(bool hardDelete = false);
        Task Rename(string newName);
        IContainer? GetParent();
    }
}