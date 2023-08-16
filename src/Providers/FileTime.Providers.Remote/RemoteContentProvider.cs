using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Providers.Remote;

public class RemoteContentProvider : ContentProviderBase, IRemoteContentProvider
{
    public RemoteContentProvider(
        ITimelessContentProvider timelessContentProvider,
        string remoteName,
        string name = "remote")
        : base(name, timelessContentProvider)
    {
    }

    //TODO implement
    public override Task<IItem> GetItemByNativePathAsync(NativePath nativePath, PointInTime pointInTime, bool forceResolve = false, AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown, ItemInitializationSettings itemInitializationSettings = default) => throw new NotImplementedException();

    public override NativePath GetNativePath(FullName fullName) => throw new NotImplementedException();

    public override FullName GetFullName(NativePath nativePath) => throw new NotImplementedException();

    public override Task<byte[]?> GetContentAsync(IElement element, int? maxLength = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public override bool CanHandlePath(NativePath path) => throw new NotImplementedException();
    public override VolumeSizeInfo? GetVolumeSizeInfo(FullName path) => throw new NotImplementedException();
}