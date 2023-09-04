using DiscUtils.Udf;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.Tools.VirtualDiskSources;

public class VirtualDiskContentReaderFactory : IContentReaderFactory<VirtualDiskContentProvider>
{
    private readonly IContentAccessorFactory _contentAccessorFactory;

    public VirtualDiskContentReaderFactory(IContentAccessorFactory contentAccessorFactory)
    {
        _contentAccessorFactory = contentAccessorFactory;
    }

    public async Task<IContentReader> CreateContentReaderAsync(IElement element)
    {
        if (element.Provider is not VirtualDiskContentProvider provider)
            throw new ArgumentException(
                "Provider must be VirtualDiskContentProvider, but it is " + element.Provider.GetType(),
                nameof(element));

        var elementNativePath = element.NativePath!;

        var supportedPath = (await provider.ParentContentProvider.GetSupportedPathPart(elementNativePath))!;

        var parentElement = (IElement) await provider.ParentContentProvider.GetItemByNativePathAsync(supportedPath, element.PointInTime);


        var contentReaderFactory = _contentAccessorFactory.GetContentReaderFactory(parentElement.Provider);
        var reader = await contentReaderFactory.CreateContentReaderAsync(parentElement);

        var readerStream = reader.AsStream();
        var discReader = new UdfReader(readerStream);

        var subPath = elementNativePath.Path.Substring(supportedPath.Path.Length + 2 + Constants.SubContentProviderRootContainer.Length);
        var fileInfo = discReader.GetFileInfo(subPath);

        var contentReader = fileInfo.Open(FileMode.Open, FileAccess.Read);

        return new VirtualDiskContentReader(contentReader, new IDisposable[] {discReader, readerStream, contentReader});
    }
}