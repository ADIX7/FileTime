using FileTime.Core.ContentAccess;
using FileTime.Core.Timeline;

namespace FileTime.Tools.VirtualDiskSources;

public class VirtualDiskContentProvider : SubContentProviderBase
{
    public VirtualDiskContentProvider(
        IContentProvider parentContentProvider, 
        ITimelessContentProvider timelessContentProvider) 
        : base(parentContentProvider, "virtual-disk", timelessContentProvider)
    {
    }
}