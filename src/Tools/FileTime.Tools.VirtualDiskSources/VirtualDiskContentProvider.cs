using DiscUtils.Udf;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Tools.VirtualDiskSources;

public class VirtualDiskContentProvider : SubContentProviderBase, IVirtualDiskContentProvider
{
    private readonly IContentAccessorFactory _contentAccessorFactory;

    public VirtualDiskContentProvider(
        IContentProvider parentContentProvider,
        ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory)
        : base(parentContentProvider, "virtual-disk", timelessContentProvider)
    {
        _contentAccessorFactory = contentAccessorFactory;
    }

    public override async Task<byte[]?> GetContentAsync(IElement element, int? maxLength = null, CancellationToken cancellationToken = default)
    {
        var elementNativePath = element.NativePath!;

        var supportedPath = await ParentContentProvider.GetSupportedPathPart(elementNativePath);
        if (supportedPath is null) return null;

        var parentItem = await ParentContentProvider.GetItemByNativePathAsync(supportedPath, element.PointInTime);
        if (parentItem is not IElement parentElement) return null;

        var contentReaderFactory = _contentAccessorFactory.GetContentReaderFactory(parentElement.Provider);
        var reader = await contentReaderFactory.CreateContentReaderAsync(parentElement);

        await using var readerStream = reader.AsStream();
        using var discReader = new UdfReader(readerStream);

        var subPath = elementNativePath.Path.Substring(supportedPath.Path.Length + 2 + Constants.SubContentProviderRootContainer.Length);
        var fileInfo = discReader.GetFileInfo(subPath);

        await using var contentReader = fileInfo.Open(FileMode.Open, FileAccess.Read);
        var data = new byte[1024 * 1024];
        var readAsync = await contentReader.ReadAsync(data, cancellationToken);

        return data[0..readAsync].ToArray();
    }

    public override VolumeSizeInfo? GetVolumeSizeInfo(FullName path)
        => ParentContentProvider.GetVolumeSizeInfo(path);
}