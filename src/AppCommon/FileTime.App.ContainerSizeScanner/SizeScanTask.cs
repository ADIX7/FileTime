using DeclarativeProperty;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Models.Extensions;
using FileTime.Core.Timeline;
using Microsoft.Extensions.Logging;

namespace FileTime.App.ContainerSizeScanner;

public class SizeScanTask : ISizeScanTask
{
    private IContainer _scanSizeOf = null!;
    private readonly IContainerScanSnapshotProvider _containerScanSnapshotProvider;
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ILogger<SizeScanTask> _logger;
    private Thread? _sizeScanThread;
    private static int _searchId = 1;
    public IContainerSizeScanContainer SizeContainer { get; private set; } = null!;

    public SizeScanTask(
        IContainerScanSnapshotProvider containerScanSnapshotProvider,
        ITimelessContentProvider timelessContentProvider,
        ILogger<SizeScanTask> logger)
    {
        _containerScanSnapshotProvider = containerScanSnapshotProvider;
        _timelessContentProvider = timelessContentProvider;
        _logger = logger;
    }

    public void Init(IContainer scanSizeOf)
    {
        _scanSizeOf = scanSizeOf;
        var name = $"{_searchId++}_{scanSizeOf.Name}";
        var randomId = ContainerScanSnapshotProvider.ContentProviderName + Constants.SeparatorChar + name;
        SizeContainer = new ContainerSizeScanContainer(_containerScanSnapshotProvider)
        {
            Name = name,
            DisplayName = scanSizeOf.DisplayName,
            FullName = new FullName(randomId),
            NativePath = new NativePath(randomId),
            Parent = new AbsolutePath(_timelessContentProvider, _containerScanSnapshotProvider),
            RealContainer = scanSizeOf
        };
    }

    public void Start()
    {
        if (_sizeScanThread != null) return;

        var sizeScanThread = new Thread(Run);
        sizeScanThread.Start();
        _sizeScanThread = sizeScanThread;
    }

    private async void Run()
    {
        try
        {
            await TraverseTree(_scanSizeOf, SizeContainer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while scanning container {ContainerName}", _scanSizeOf.Name);
        }
    }

    //TODO: make static
    private async Task TraverseTree(
        IContainer realContainer,
        IContainerSizeScanContainer container)
    {
        var resolvedItems = new List<IItem>(realContainer.Items.Count);
        foreach (var item in realContainer.Items)
        {
            var resolvedItem = await item.ResolveAsync();
            resolvedItems.Add(resolvedItem);
        }

        foreach (var element in resolvedItems.OfType<IElement>())
        {
            var fileExtension = element.GetExtension<FileExtension>();
            if (fileExtension?.Size is not { } size) continue;

            var childName = container.FullName!.GetChild(element.Name).Path;
            await container.AddSizeSourceAsync(new DeclarativeProperty<long>(size));
            container.Items.Add(new AbsolutePath(
                _timelessContentProvider,
                PointInTime.Present,
                new FullName(childName),
                AbsolutePathType.Element));
        }

        foreach (var childContainer in resolvedItems.OfType<IContainer>())
        {
            var childName = container.FullName!.GetChild(childContainer.Name).Path;
            var childSearchContainer = new ContainerSizeScanContainer(_containerScanSnapshotProvider)
            {
                Name = childContainer.Name,
                DisplayName = childContainer.DisplayName,
                FullName = new FullName(childName),
                NativePath = new NativePath(childName),
                Parent = new AbsolutePath(_timelessContentProvider, container),
                RealContainer = childContainer
            };

            container.ChildContainers.Add(childSearchContainer);
            await container.AddSizeSourceAsync(childSearchContainer.Size);
            container.Items.Add(new AbsolutePath(
                _timelessContentProvider,
                PointInTime.Present,
                new FullName(childName),
                AbsolutePathType.Container));

            await TraverseTree(childContainer, childSearchContainer);
        }
    }
}