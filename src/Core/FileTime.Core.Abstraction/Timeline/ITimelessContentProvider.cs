using DeclarativeProperty;
using FileTime.Core.Enums;
using FileTime.Core.Models;

namespace FileTime.Core.Timeline;

public interface ITimelessContentProvider
{
    IDeclarativeProperty<PointInTime> CurrentPointInTime { get; }

    Task<IItem> GetItemByFullNameAsync(FullName fullName,
        PointInTime? pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default);

    Task<IItem?> GetItemByNativePathAsync(NativePath nativePath, PointInTime? pointInTime = null);
    ValueTask<FullName?> GetFullNameByNativePathAsync(NativePath nativePath);
    ValueTask<NativePath?> GetNativePathByFullNameAsync(FullName fullName);
}