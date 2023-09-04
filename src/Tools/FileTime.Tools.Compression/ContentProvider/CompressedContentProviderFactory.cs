using FileTime.Core.ContentAccess;
using FileTime.Core.Timeline;

namespace FileTime.Tools.Compression.ContentProvider;

public class CompressedContentProviderFactory : ICompressedContentProviderFactory
{
    private readonly ITimelessContentProvider _timelessContentProvider;

    public CompressedContentProviderFactory(ITimelessContentProvider timelessContentProvider)
    {
        _timelessContentProvider = timelessContentProvider;
    }

    public ICompressedContentProvider Create(IContentProvider parentContentProvider)
    {
        return new CompressedContentProvider(parentContentProvider, _timelessContentProvider);
    }
}