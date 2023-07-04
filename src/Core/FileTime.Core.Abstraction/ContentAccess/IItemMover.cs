using FileTime.Core.Models;

namespace FileTime.Core.ContentAccess;

public interface IItemMover
{
    Task RenameAsync(IContentProvider contentProvider, FullName fullName, FullName newPath);
}

public interface IItemMover<in TContentProvider> : IItemMover where TContentProvider : IContentProvider
{
    Task RenameAsync(TContentProvider contentProvider, FullName fullName, FullName newPath);

    async Task IItemMover.RenameAsync(IContentProvider contentProvider, FullName fullName, FullName newPath)
    {
        if(contentProvider is not TContentProvider provider) 
            throw new ArgumentException(
                $"Content provider ({contentProvider.GetType()}) is not the required type ({typeof(TContentProvider)}) ", 
                nameof(contentProvider)
            );

        await RenameAsync(provider, fullName, newPath);
    }
}