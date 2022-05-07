using FileTime.Core.Behaviors;
using FileTime.Core.Enums;
using FileTime.Core.Models;

namespace FileTime.Core.Services;

public interface IContentProvider : IContainer, IOnContainerEnter
{
    Task<IItem> GetItemByFullNameAsync(
        FullName fullName,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default);

    Task<IItem> GetItemByNativePathAsync(
        NativePath nativePath,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default);

    Task<List<IAbsolutePath>> GetItemsByContainerAsync(FullName fullName);
}