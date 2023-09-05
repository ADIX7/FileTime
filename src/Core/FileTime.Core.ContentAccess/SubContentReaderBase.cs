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

    protected async Task<ParentElementReaderContext> GetParentElementReaderAsync(IElement element, SubContentProviderBase provider)
        => await Helper.GetParentElementReaderAsync(_contentAccessorFactory, element, provider.ParentContentProvider);
}