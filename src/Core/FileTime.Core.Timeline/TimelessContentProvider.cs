using System.Reactive.Subjects;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Core.Timeline;

public class TimelessContentProvider : ITimelessContentProvider
{
    private readonly IContentProviderRegistry _contentProviderRegistry;
    private readonly Lazy<IRootContentProvider> _rootContentProvider;

    public BehaviorSubject<PointInTime> CurrentPointInTime { get; } = new(PointInTime.Present);

    public TimelessContentProvider(
        IContentProviderRegistry contentProviderRegistry,
        IServiceProvider serviceProvider
    )
    {
        _contentProviderRegistry = contentProviderRegistry;
        _rootContentProvider = new Lazy<IRootContentProvider>(serviceProvider.GetRequiredService<IRootContentProvider>);
    }

    public async Task<IItem> GetItemByFullNameAsync(FullName fullName, PointInTime? pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default)
    {
        //TODO time modifications
        var contentProviderName = fullName.Path.Split(Constants.SeparatorChar).FirstOrDefault();
        if (contentProviderName == "") return _rootContentProvider.Value;

        var contentProvider = _contentProviderRegistry.ContentProviders.FirstOrDefault(p => p.Name == contentProviderName);

        if (contentProvider is null)
            throw new Exception($"No content provider is found for name '{contentProviderName}'");

        return await contentProvider.GetItemByFullNameAsync(fullName, pointInTime ?? PointInTime.Present,
            forceResolve, forceResolvePathType,
            itemInitializationSettings);
    }

    public async Task<IItem?> GetItemByNativePathAsync(NativePath nativePath, PointInTime? pointInTime = null)
    {
        foreach (var contentProvider in _contentProviderRegistry.ContentProviders)
        {
            if (!contentProvider.CanHandlePath(nativePath)) continue;

            return await contentProvider.GetItemByNativePathAsync(nativePath, pointInTime ?? PointInTime.Present);
        }

        return null;
    }

    public FullName? GetFullNameByNativePath(NativePath nativePath)
    {
        foreach (var contentProvider in _contentProviderRegistry.ContentProviders)
        {
            if (!contentProvider.CanHandlePath(nativePath)) continue;

            return contentProvider.GetFullName(nativePath);
        }

        return null;
    }

    public NativePath? GetNativePathByFullName(FullName fullName)
    {
        foreach (var contentProvider in _contentProviderRegistry.ContentProviders)
        {
            if (!contentProvider.CanHandlePath(fullName)) continue;

            return contentProvider.GetNativePath(fullName);
        }

        return null;
    }
}