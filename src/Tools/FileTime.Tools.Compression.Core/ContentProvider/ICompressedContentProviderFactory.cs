using FileTime.Core.ContentAccess;

namespace FileTime.Tools.Compression.ContentProvider;

public interface ICompressedContentProviderFactory
{
    ICompressedContentProvider Create(IContentProvider parentContentProvider);
}