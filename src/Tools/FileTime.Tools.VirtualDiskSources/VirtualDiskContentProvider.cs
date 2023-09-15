using DiscUtils.Udf;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Tools.VirtualDiskSources;

public sealed class VirtualDiskContentProvider : SubContentProviderBase, IVirtualDiskContentProvider
{
    private readonly ITimelessContentProvider _timelessContentProvider;

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
        _timelessContentProvider = timelessContentProvider;
    }

    public override async Task<byte[]?> GetContentAsync(IElement element, int? maxLength = null, CancellationToken cancellationToken = default)
    {
        var parentElementContext = await GetParentElementReaderAsync(element);
        var reader = parentElementContext.ContentReader;
        var subPath = parentElementContext.SubNativePath.Path;
        
        await using var readerStream = reader.GetStream();
        using var discReader = new UdfReader(readerStream);

        var fileInfo = discReader.GetFileInfo(subPath);

        await using var contentReader = fileInfo.Open(FileMode.Open, FileAccess.Read);
        var data = new byte[1024 * 1024];
        var readAsync = await contentReader.ReadAsync(data, cancellationToken);

        return data[..readAsync].ToArray();
    }


    public override async ValueTask<VolumeSizeInfo?> GetVolumeSizeInfoAsync(FullName path)
    {
        var item = await GetItemByFullNameAsync(path, _timelessContentProvider.CurrentPointInTime.Value!);
        var parentElement = await GetParentElementAsync(item);
        return new VolumeSizeInfo(parentElement.Size, 0);
    }
}