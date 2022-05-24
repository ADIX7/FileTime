using FileTime.Core.Models;

namespace FileTime.Core.ContentAccess;

public interface IItemCreator
{
    Task CreateContainerAsync(IContentProvider contentProvider, FullName fullName);
    Task CreateElementAsync(IContentProvider contentProvider, FullName fullName);
}

public interface IItemCreator<in TContentProvider> : IItemCreator where TContentProvider : IContentProvider
{
    Task CreateContainerAsync(TContentProvider contentProvider, FullName fullName);
    Task CreateElementAsync(TContentProvider contentProvider, FullName fullName);
}