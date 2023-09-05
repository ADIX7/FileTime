using DiscUtils.Udf;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.Tools.VirtualDiskSources;

public sealed class VirtualDiskContentReaderFactory : SubContentReaderBase<VirtualDiskContentProvider>
{
    public VirtualDiskContentReaderFactory(IContentAccessorFactory contentAccessorFactory) 
        : base(contentAccessorFactory)
    {
    }

    public override async Task<IContentReader> CreateContentReaderAsync(IElement element)
    {
        if (element.Provider is not VirtualDiskContentProvider provider)
            throw new ArgumentException(
                $"Provider must be {nameof(VirtualDiskContentProvider)}, but it is " + element.Provider.GetType(),
                nameof(element));
        
        var parentElementReaderContext = await GetParentElementReaderAsync(element, provider);
        var reader = parentElementReaderContext.ContentReader;
        var subPath = parentElementReaderContext.SubNativePath;

        var readerStream = reader.AsStream();
        var discReader = new UdfReader(readerStream);

        var fileInfo = discReader.GetFileInfo(subPath.Path);

        var contentReader = fileInfo.Open(FileMode.Open, FileAccess.Read);

        return new VirtualDiskContentReader(contentReader, new IDisposable[] {discReader, readerStream, contentReader});
    }
}