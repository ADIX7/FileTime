using FileTime.Core.Models;

namespace FileTime.Core.ContentAccess;

public abstract class SubContentReaderBase<TContentProvider> : IContentReaderFactory<TContentProvider> where TContentProvider : IContentProvider
{
    private readonly IContentAccessorFactory _contentAccessorFactory;

    protected SubContentReaderBase(IContentAccessorFactory contentAccessorFactory)
    {
        _contentAccessorFactory = contentAccessorFactory;
    }

    public abstract Task<IContentReader> CreateContentReaderAsync(IElement element);

    protected async Task<ParentElementReaderContext> GetParentElementReaderAsync(IItem item, SubContentProviderBase provider)
        => await Helper.GetParentElementReaderAsync(_contentAccessorFactory, item, provider.ParentContentProvider);

    protected async Task<IElement> GetParentElementAsync(IItem item, SubContentProviderBase provider)
        => await Helper.GetParentElementAsync(item, provider.ParentContentProvider);
}