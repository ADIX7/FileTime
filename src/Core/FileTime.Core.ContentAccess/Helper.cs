using FileTime.Core.Models;

namespace FileTime.Core.ContentAccess;

internal static class Helper
{
    internal static async Task<ParentElementReaderContext> GetParentElementReaderAsync(
        IContentAccessorFactory contentAccessorFactory, 
        IElement element, 
        IContentProvider parentContentProvider)
    {
        var elementNativePath = element.NativePath!;
        var supportedPath = (await parentContentProvider.GetSupportedPathPart(elementNativePath))!;
        var parentElement = (IElement) await parentContentProvider.GetItemByNativePathAsync(supportedPath, element.PointInTime);

        var contentReaderFactory = contentAccessorFactory.GetContentReaderFactory(parentElement.Provider);
        var reader = await contentReaderFactory.CreateContentReaderAsync(parentElement);
        var subPath = new NativePath(elementNativePath.Path.Substring(supportedPath.Path.Length + 2 + Constants.SubContentProviderRootContainer.Length));

        return new ParentElementReaderContext(reader, subPath);
    }
}