using FileTime.Core.ContentAccess;
using FileTime.Core.Timeline;

namespace FileTime.Tools.Compression.ContentProvider;

public sealed class CompressedContentProviderFactory : ICompressedContentProviderFactory
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly IContentAccessorFactory _contentAccessorFactory;

    public CompressedContentProviderFactory(
        ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory)
    {
        _timelessContentProvider = timelessContentProvider;
        _contentAccessorFactory = contentAccessorFactory;
    }

    public ICompressedContentProvider Create(IContentProvider parentContentProvider) 
        => new CompressedContentProvider(_timelessContentProvider, _contentAccessorFactory, parentContentProvider);
}