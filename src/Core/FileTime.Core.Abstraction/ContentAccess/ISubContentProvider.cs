using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.ContentAccess;

public interface ISubContentProvider
{
    Task<IItem?> GetItemByFullNameAsync(
        IElement parentElement,
        FullName itemPath,
        PointInTime pointInTime,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default);

    Task<bool> CanHandleAsync(IElement parentElement);
}