using FileTime.Core.Models;

namespace FileTime.Core.ContentAccess;

public interface IItemDeleter
{
    Task DeleteAsync(IContentProvider contentProvider, FullName fullName);
}

public interface IItemDeleter<in TContentProvider> : IItemDeleter where TContentProvider : IContentProvider
{
    Task DeleteAsync(TContentProvider contentProvider, FullName fullName);

    async Task IItemDeleter.DeleteAsync(IContentProvider contentProvider, FullName fullName)
    {
        if (contentProvider is not TContentProvider provider)
        {
            throw new ArgumentException(
                $"Content provider ({contentProvider.GetType()}) is not the required type ({typeof(TContentProvider)}) ",
                nameof(contentProvider)
            );
        }

        await DeleteAsync(provider, fullName);
    }
}