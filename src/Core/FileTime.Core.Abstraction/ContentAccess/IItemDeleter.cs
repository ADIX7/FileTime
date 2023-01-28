using FileTime.Core.Models;

namespace FileTime.Core.ContentAccess;

public interface IItemDeleter
{
    Task DeleteAsync(IContentProvider contentProvider, FullName fullName);
}

public interface IItemDeleter<in TContentProvider> : IItemDeleter where TContentProvider : IContentProvider
{
    Task DeleteAsync(TContentProvider contentProvider, FullName fullName);
}