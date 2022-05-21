using FileTime.Core.Enums;
using FileTime.Core.Timeline;

namespace FileTime.Core.Models;

public class AbsolutePath
{
    public ITimelessContentProvider TimelessProvider { get; }
    public PointInTime PointInTime { get; }

    public FullName Path { get; }
    public AbsolutePathType Type { get; }

    public AbsolutePath(
        ITimelessContentProvider timelessProvider,
        PointInTime pointInTime,
        FullName path,
        AbsolutePathType type)
    {
        TimelessProvider = timelessProvider;
        Path = path;
        Type = type;
        PointInTime = pointInTime;
    }

    public AbsolutePath(ITimelessContentProvider timelessProvider, IItem item)
    {
        TimelessProvider = timelessProvider;
        PointInTime = item.PointInTime;
        Path = item.FullName ?? throw new ArgumentException($"{nameof(item.FullName)} can not be null.", nameof(item));
        Type = item.Type;
    }

    public async Task<IItem> ResolveAsync(bool forceResolve = false,
        ItemInitializationSettings itemInitializationSettings = default)
    {
        return await TimelessProvider.GetItemByFullNameAsync(Path, PointInTime, forceResolve, Type,
            itemInitializationSettings);
    }

    public async Task<IItem?> ResolveAsyncSafe(bool forceResolve = false,
        ItemInitializationSettings itemInitializationSettings = default)
    {
        try
        {
            return await ResolveAsync(forceResolve, itemInitializationSettings);
        }
        catch
        {
            return null;
        }
    }
}