using FileTime.Core.ContentAccess;
using FileTime.Core.Timeline;

namespace FileTime.Tools.Compression.ContentProvider;

public sealed class CompressedContentProviderFactory(ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory)
    : ICompressedContentProviderFactory
{
    public ICompressedContentProvider Create(IContentProvider parentContentProvider) 
        => new CompressedContentProvider(timelessContentProvider, contentAccessorFactory, parentContentProvider);
}