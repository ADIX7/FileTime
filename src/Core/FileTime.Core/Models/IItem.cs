using FileTime.Core.Providers;

namespace FileTime.Core.Models
{
    public interface IItem
    {
        string Name { get; }
        string? FullName { get; }
        bool IsHidden { get; }
        IContentProvider Provider { get; }
        void Delete();
    }
}