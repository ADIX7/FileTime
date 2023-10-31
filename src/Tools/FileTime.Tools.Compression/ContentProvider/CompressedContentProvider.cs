using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using SharpCompress.Archives;

namespace FileTime.Tools.Compression.ContentProvider;

public sealed class CompressedContentProvider(ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory,
        IContentProvider parentContentProvider)
    : SubContentProviderBase(timelessContentProvider,
        contentAccessorFactory,
        parentContentProvider,
        "compression"), ICompressedContentProvider
{
    public override async Task<byte[]?> GetContentAsync(IElement element, int? maxLength = null, CancellationToken cancellationToken = default)
    {
        var parentElementContext = await GetParentElementReaderAsync(element);
        var reader = parentElementContext.ContentReader;
        var subPath = parentElementContext.SubNativePath.Path;

        await using var readerStream = reader.GetStream();
        using var archive = ArchiveFactory.Open(readerStream);

        var entry = archive.Entries.First(e => e.Key == subPath);
        await using var contentReader = entry.OpenEntryStream();

        var data = new byte[1024 * 1024];
        var readAsync = await contentReader.ReadAsync(data, cancellationToken);

        return data[..readAsync].ToArray();
    }

    public override async ValueTask<VolumeSizeInfo?> GetVolumeSizeInfoAsync(FullName path)
    {
        var item = await GetItemByFullNameAsync(path, timelessContentProvider.CurrentPointInTime.Value!);
        var parentElement = await GetParentElementAsync(item);
        return new VolumeSizeInfo(parentElement.Size, 0);
    }
}