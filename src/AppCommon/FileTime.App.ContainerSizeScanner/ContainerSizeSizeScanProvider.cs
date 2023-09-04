using FileTime.App.Core.Exceptions;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using InitableService;

namespace FileTime.App.ContainerSizeScanner;

public class ContainerSizeSizeScanProvider : ContentProviderBase, IContainerSizeScanProvider
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly List<ISizeScanTask> _sizeScanTasks = new();
    internal const string ContentProviderName = "container-size-scan";

    public ContainerSizeSizeScanProvider(
        ITimelessContentProvider timelessContentProvider,
        IServiceProvider serviceProvider)
        : base(ContentProviderName, timelessContentProvider)
    {
        _timelessContentProvider = timelessContentProvider;
        _serviceProvider = serviceProvider;
    }

    public override async Task<IItem> GetItemByFullNameAsync(
        FullName fullName,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default
    )
    {
        if (fullName.Path == ContentProviderName)
            return this;

        var pathParts = fullName.Path.Split(Constants.SeparatorChar);

        var item = _sizeScanTasks.FirstOrDefault(t => t.SizeSizeScanContainer.Name == pathParts[1])?.SizeSizeScanContainer;

        if (pathParts.Length == 2)
            return item ?? throw new ItemNotFoundException(fullName);

        for (var i = 2; i < pathParts.Length - 1 && item != null; i++)
        {
            var childName = pathParts[i];
            item = item.ChildContainers.FirstOrDefault(c => c.Name == childName);
        }

        if (item is not null)
        {
            var childItem = item.SizeItems.FirstOrDefault(c => c.Name == pathParts[^1]);
            if (childItem is not null) return childItem;

            /*var childName = item.RealContainer.FullName?.GetChild(pathParts[^1]);
            if (childName is null) throw new ItemNotFoundException(fullName);

            return await _timelessContentProvider.GetItemByFullNameAsync(
                childName,
                pointInTime,
                forceResolve,
                forceResolvePathType,
                itemInitializationSettings
            );*/
        }

        throw new ItemNotFoundException(fullName);
    }

    public override async Task<IItem> GetItemByNativePathAsync(
        NativePath nativePath,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default
    ) =>
        await GetItemByFullNameAsync(
            new FullName(nativePath.Path),
            pointInTime,
            forceResolve,
            forceResolvePathType,
            itemInitializationSettings
        );

    public override ValueTask<NativePath> GetNativePathAsync(FullName fullName)
        => ValueTask.FromResult(new NativePath(fullName.Path));

    public override FullName GetFullName(NativePath nativePath)
        => new(nativePath.Path);

    public override Task<byte[]?> GetContentAsync(
            IElement element,
            int? maxLength = null,
            CancellationToken cancellationToken = default)
        //TODO read from original source
        => Task.FromResult((byte[]?) null);

    public override Task<bool> CanHandlePathAsync(NativePath path)
        => Task.FromResult(path.Path.StartsWith(ContentProviderName));

    public override VolumeSizeInfo? GetVolumeSizeInfo(FullName path) => null;

    public override ValueTask<NativePath?> GetSupportedPathPart(NativePath nativePath)
        => ValueTask.FromResult<NativePath?>(nativePath);

    public ISizeScanTask StartSizeScan(IContainer scanSizeOf)
    {
        var searchTask = _serviceProvider
            .GetInitableResolver(scanSizeOf)
            .GetRequiredService<ISizeScanTask>();

        _sizeScanTasks.Add(searchTask);
        searchTask.Start();
        Items.Add(new AbsolutePath(_timelessContentProvider, searchTask.SizeSizeScanContainer));

        return searchTask;
    }

    public Task ExitAsync(CancellationToken token = default)
    {
        foreach (var sizeScanTask in _sizeScanTasks)
        {
            try
            {
                sizeScanTask.Stop();
            }
            catch
            {
            }
        }

        return Task.CompletedTask;
    }
}