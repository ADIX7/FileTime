using FileTime.Core.Enums;
using FileTime.Core.Services;

namespace FileTime.Core.Models;

public class AbsolutePath : IAbsolutePath
{
    public IContentProvider ContentProvider { get; }
    public IContentProvider? VirtualContentProvider { get; }

    public FullName Path { get; }
    public AbsolutePathType Type { get; }

    public AbsolutePath(IContentProvider contentProvider, FullName path, AbsolutePathType type, IContentProvider? virtualContentProvider = null)
    {
        ContentProvider = contentProvider;
        Path = path;
        VirtualContentProvider = virtualContentProvider;
        Type = type;
    }

    public AbsolutePath(IItem item, IContentProvider? virtualContentProvider = null)
    {
        ContentProvider = item.Provider;
        Path = item.FullName ?? throw new ArgumentException($"{nameof(item.FullName)} can not be null.", nameof(item));
        VirtualContentProvider = virtualContentProvider;
        Type = item.Type;
    }

    public async Task<IItem> ResolveAsync(bool forceResolve = false, ItemInitializationSettings itemInitializationSettings = default)
    {
        var provider = VirtualContentProvider ?? ContentProvider;
        return await provider.GetItemByFullNameAsync(Path, forceResolve, Type, itemInitializationSettings);
    }

    public async Task<IItem?> ResolveAsyncSafe(bool forceResolve = false, ItemInitializationSettings itemInitializationSettings = default)
    {
        try
        {
            return await ResolveAsync(forceResolve, itemInitializationSettings);
        }
        catch { return null; }
    }
}