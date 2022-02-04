using FileTime.Core.Providers;

namespace FileTime.Core.Models
{
    public interface IItem : IDisposable
    {
        string Name { get; }
        string? FullName { get; }
        bool IsHidden { get; }
        bool IsDisposed { get; }
        SupportsDelete CanDelete { get; }
        bool CanRename { get; }
        IContentProvider Provider { get; }
        Task Delete(bool hardDelete = false);
        Task Rename(string newName);
        IContainer? GetParent();
    }
}