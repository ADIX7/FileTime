using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using SharpCompress.Archives;

namespace FileTime.Tools.Compression.ContentProvider;

public sealed class CompressedContentReaderFactory : SubContentReaderBase<CompressedContentProvider>
{

    public CompressedContentReaderFactory(IContentAccessorFactory contentAccessorFactory) 
        : base(contentAccessorFactory)
    {
    }
    public override async Task<IContentReader> CreateContentReaderAsync(IElement element)
    {
        if (element.Provider is not CompressedContentProvider provider)
            throw new ArgumentException(
                $"Provider must be {nameof(CompressedContentProvider)}, but it is " + element.Provider.GetType(),
                nameof(element));
        
        var parentElementReaderContext = await GetParentElementReaderAsync(element, provider);
        var reader = parentElementReaderContext.ContentReader;
        var subPath = parentElementReaderContext.SubNativePath;
        
        var readerStream = reader.GetStream();
        var archive = ArchiveFactory.Open(readerStream);

        var entry = archive.Entries.First(e => e.Key == subPath.Path);
        
        var disposables = new IDisposable[] {archive, readerStream}; 
        
        return new CompressedContentReader(entry, disposables);
    }
}