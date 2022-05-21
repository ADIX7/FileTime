using FileTime.Core.Behaviors;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.Services;

public interface IContentProvider : IContainer, IOnContainerEnter
{
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

    Task<List<AbsolutePath>> GetItemsByContainerAsync(FullName fullName, PointInTime pointInTime);
    NativePath GetNativePath(FullName fullName);

    Task<byte[]?> GetContentAsync(IElement element, int? maxLength = null, CancellationToken cancellationToken = default);
}