using FileTime.Core.Models;

namespace FileTime.Core.ContentAccess;

public abstract class ItemCreatorBase<TContentProvider> : IItemCreator<TContentProvider>
    where TContentProvider : IContentProvider
{
    public async Task CreateContainerAsync(IContentProvider contentProvider, FullName fullName)
        => await CreateContainerAsync((TContentProvider)contentProvider, fullName);

    public async Task CreateElementAsync(IContentProvider contentProvider, FullName fullName)
        => await CreateElementAsync((TContentProvider)contentProvider, fullName);

    public abstract Task CreateContainerAsync(TContentProvider contentProvider, FullName fullName);

    public abstract Task CreateElementAsync(TContentProvider contentProvider, FullName fullName);
}