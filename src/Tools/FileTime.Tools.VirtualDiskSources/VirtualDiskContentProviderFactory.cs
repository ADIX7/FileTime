using FileTime.Core.ContentAccess;
using FileTime.Core.Timeline;

namespace FileTime.Tools.VirtualDiskSources;

public sealed class VirtualDiskContentProviderFactory(ITimelessContentProvider timelessContentProvider,
        IContentAccessorFactory contentAccessorFactory)
    : IVirtualDiskContentProviderFactory
{
    public IVirtualDiskContentProvider Create(IContentProvider parentContentProvider)
        => new VirtualDiskContentProvider(timelessContentProvider, contentAccessorFactory, parentContentProvider);
}