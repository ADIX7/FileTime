using DiscUtils.Udf;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Tools.VirtualDiskSources;

public sealed class VirtualDiskContentProvider : SubContentProviderBase, IVirtualDiskContentProvider
{
    private readonly IContentAccessorFactory _contentAccessorFactory;

    public VirtualDiskContentProvider(
        ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory,
        IContentProvider parentContentProvider)
        : base(
            timelessContentProvider, 
            contentAccessorFactory, 
            parentContentProvider, 
            "virtual-disk"
        )
    {
        _contentAccessorFactory = contentAccessorFactory;
    }

    public override async Task<byte[]?> GetContentAsync(IElement element, int? maxLength = null, CancellationToken cancellationToken = default)
    {
        var parentElementContext = await GetParentElementReaderAsync(element);
        var reader = parentElementContext.ContentReader;
        var subPath = parentElementContext.SubNativePath.Path;
        
        await using var readerStream = reader.AsStream();
        using var discReader = new UdfReader(readerStream);

        var fileInfo = discReader.GetFileInfo(subPath);

        await using var contentReader = fileInfo.Open(FileMode.Open, FileAccess.Read);
        var data = new byte[1024 * 1024];
        var readAsync = await contentReader.ReadAsync(data, cancellationToken);

        return data[..readAsync].ToArray();
    }

    public override VolumeSizeInfo? GetVolumeSizeInfo(FullName path)
        => ParentContentProvider.GetVolumeSizeInfo(path);
}