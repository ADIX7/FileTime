using System.Reactive.Subjects;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Core.Timeline;

public class TimelessContentProvider : ITimelessContentProvider
{
    private readonly Lazy<List<IContentProvider>> _contentProviders;

    public BehaviorSubject<PointInTime> CurrentPointInTime { get; } =
        new BehaviorSubject<PointInTime>(PointInTime.Present);

    public TimelessContentProvider(IServiceProvider serviceProvider)
    {
        _contentProviders =
            new Lazy<List<IContentProvider>>(() => serviceProvider.GetServices<IContentProvider>().ToList());
    }

    public async Task<IItem> GetItemByFullNameAsync(FullName fullName, PointInTime? pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default)
    {
        //TODO time modifications
        var contentProviderName = fullName.Path.Split(Constants.SeparatorChar).FirstOrDefault();
        var contentProvider = _contentProviders.Value.FirstOrDefault(p => p.Name == contentProviderName);

        if (contentProvider is null)
            throw new Exception($"No content provider is found for name '{contentProviderName}'");

        return await contentProvider.GetItemByFullNameAsync(fullName, pointInTime ?? PointInTime.Present,
            forceResolve, forceResolvePathType,
            itemInitializationSettings);
    }
}