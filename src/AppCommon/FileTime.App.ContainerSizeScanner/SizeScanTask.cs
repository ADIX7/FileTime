using DeclarativeProperty;
using FileTime.Core.Models;
using FileTime.Core.Models.Extensions;
using FileTime.Core.Timeline;
using Microsoft.Extensions.Logging;

namespace FileTime.App.ContainerSizeScanner;

public class SizeScanTask : ISizeScanTask
{
    private bool _cancelled;
    private int _processedItems;
    private ulong _processedItemsTotal;
    private IContainer _scanSizeOf = null!;
    private readonly IContainerSizeScanProvider _containerSizeScanProvider;
    private readonly ITimelessContentProvider _timelessContentProvider;
    private readonly ILogger<SizeScanTask> _logger;
    private Thread? _sizeScanThread;
    private static int _searchId = 1;
    private readonly DeclarativeProperty<string> _containerStatus = new();
    private readonly IDeclarativeProperty<string> _containerStatusDebounced;
    public ISizeScanContainer SizeSizeScanContainer { get; private set; } = null!;
    public bool IsRunning { get; private set; }

    public SizeScanTask(
        IContainerSizeScanProvider containerSizeScanProvider,
        ITimelessContentProvider timelessContentProvider,
        ILogger<SizeScanTask> logger)
    {
        _containerSizeScanProvider = containerSizeScanProvider;
        _timelessContentProvider = timelessContentProvider;
        _logger = logger;
        _containerStatusDebounced = _containerStatus.Debounce(TimeSpan.FromMilliseconds(250));
    }

    public void Init(IContainer scanSizeOf)
    {
        _scanSizeOf = scanSizeOf;
        var name = $"{_searchId++}_{scanSizeOf.Name}";
        var randomId = ContainerSizeSizeScanProvider.ContentProviderName + Constants.SeparatorChar + name;
        SizeSizeScanContainer = new SizeScanContainer(_timelessContentProvider)
        {
            Name = name,
            DisplayName = scanSizeOf.DisplayName,
            FullName = new FullName(randomId),
            NativePath = new NativePath(randomId),
            Parent = new AbsolutePath(_timelessContentProvider, _containerSizeScanProvider),
            RealContainer = scanSizeOf,
            Provider = _containerSizeScanProvider,
            Status = _containerStatusDebounced,
            SizeScanTask = this
        };
    }

    public void Start()
    {
        if (_sizeScanThread != null) return;

        var sizeScanThread = new Thread(Run);
        sizeScanThread.Start();
        _sizeScanThread = sizeScanThread;
    }

    public void Stop() => _cancelled = true;

    private async void Run()
    {
        try
        {
            IsRunning = true;
            await SizeSizeScanContainer.StartLoadingAsync();
            await TraverseTree(_scanSizeOf, SizeSizeScanContainer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while scanning container {ContainerName}", _scanSizeOf.Name);
            SizeSizeScanContainer.Exceptions.Add(ex);
        }

        IsRunning = false;
        await SizeSizeScanContainer.StopLoadingAsync();
    }

    //TODO: make static
    private async Task TraverseTree(
        IContainer realContainer,
        ISizeScanContainer sizeScanContainer)
    {
        if (_cancelled) return;

        await realContainer.WaitForLoaded();
        var resolvedItems = new List<IItem>(realContainer.Items.Count);
        foreach (var item in realContainer.Items)
        {
            var resolvedItem = await item.ResolveAsync();
            resolvedItems.Add(resolvedItem);
        }

        foreach (var element in resolvedItems.OfType<IElement>())
        {
            if (_cancelled) return;

            var fileExtension = element.GetExtension<FileExtension>();
            var size = fileExtension?.Size ?? 0;

            var sizeProperty = new DeclarativeProperty<long>(size);
            var childName = sizeScanContainer.FullName!.GetChild(element.Name).Path;

            var childElement = new SizeScanElement
            {
                Name = element.Name,
                DisplayName = element.DisplayName,
                FullName = new FullName(childName),
                NativePath = new NativePath(childName),
                Parent = new AbsolutePath(_timelessContentProvider, sizeScanContainer),
                Provider = _containerSizeScanProvider,
                Size = sizeProperty
            };
            await sizeScanContainer.AddSizeChildAsync(childElement);

            _processedItems++;
            _processedItemsTotal++;
        }

        foreach (var childContainer in resolvedItems.OfType<IContainer>())
        {
            if (_cancelled) return;

            var childName = sizeScanContainer.FullName!.GetChild(childContainer.Name).Path;
            var childSearchContainer = new SizeScanContainer(_timelessContentProvider)
            {
                Name = childContainer.Name,
                DisplayName = childContainer.DisplayName,
                FullName = new FullName(childName),
                NativePath = new NativePath(childName),
                Parent = new AbsolutePath(_timelessContentProvider, sizeScanContainer),
                RealContainer = childContainer,
                Provider = _containerSizeScanProvider,
                Status = _containerStatusDebounced,
                SizeScanTask = this
            };

            await sizeScanContainer.AddSizeChildAsync(childSearchContainer);

            await TraverseTree(childContainer, childSearchContainer);
        }

        _processedItems++;
        _processedItemsTotal++;
        await _containerStatus.SetValue("Items processed: " + _processedItemsTotal);

        if (_processedItems > 1000)
        {
            _processedItems = 0;
            //Let some time for the UI to refresh
            await Task.Delay(1000);
        }
    }
}