using System.Reactive.Subjects;
using FileTime.Core.Enums;
using FileTime.Core.Models;

namespace FileTime.Core.Timeline;

public interface ITimelessContentProvider
{
    BehaviorSubject<PointInTime> CurrentPointInTime { get; }

    Task<IItem> GetItemByFullNameAsync(FullName fullName,
        PointInTime? pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default);
}