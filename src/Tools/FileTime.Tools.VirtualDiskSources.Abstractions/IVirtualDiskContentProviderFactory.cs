using FileTime.Core.ContentAccess;

namespace FileTime.Tools.VirtualDiskSources;

public interface IVirtualDiskContentProviderFactory
{
    IVirtualDiskContentProvider Create(IContentProvider parentContentProvider);
}