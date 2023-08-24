using FileTime.Core.Behaviors;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.ContentAccess;

public interface IContentProvider : IContainer, IOnContainerEnter
{
    bool SupportsContentStreams { get; }

    Task<IItem> GetItemByFullNameAsync(
        FullName fullName,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default);

    Task<IItem> GetItemByNativePathAsync(NativePath nativePath,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default);

    ValueTask<NativePath> GetNativePathAsync(FullName fullName);
    FullName GetFullName(NativePath nativePath);

    Task<byte[]?> GetContentAsync(IElement element, int? maxLength = null, CancellationToken cancellationToken = default);
    Task<bool> CanHandlePathAsync(NativePath path);
    Task<bool> CanHandlePathAsync(FullName path);
    VolumeSizeInfo? GetVolumeSizeInfo(FullName path);
}