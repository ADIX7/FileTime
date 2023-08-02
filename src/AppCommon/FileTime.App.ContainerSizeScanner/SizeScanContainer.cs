using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DeclarativeProperty;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Models.ContainerTraits;
using FileTime.Core.Timeline;

namespace FileTime.App.ContainerSizeScanner;

//TODO: create readonly version
public record SizeScanContainer : ISizeScanContainer, IEscHandlerContainer
{
    private readonly ReadOnlyExtensionCollection _readOnlyExtensions;
    private readonly BehaviorSubject<bool> _isLoading = new(false);
    private readonly CombineProperty<long, long> _size;
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public required FullName? FullName { get; init; }
    public required NativePath? NativePath { get; init; }
    public required AbsolutePath? Parent { get; init; }
    public required IContentProvider Provider { get; init; }
    public bool IsHidden => false;
    public bool IsExists => true;
    public DateTime? CreatedAt { get; } = DateTime.Now;
    public SupportsDelete CanDelete => SupportsDelete.True;
    public bool CanRename => false;
    public string? Attributes => null;
    public AbsolutePathType Type => AbsolutePathType.Container;
    public PointInTime PointInTime => PointInTime.Present;
    public ObservableCollection<Exception> Exceptions { get; } = new();
    public ExtensionCollection Extensions { get; } = new();
    ReadOnlyExtensionCollection IItem.Extensions => _readOnlyExtensions;
    public IItem WithParent(AbsolutePath parent) => this with {Parent = parent};

    public ObservableCollection<AbsolutePath> Items { get; } = new();
    public IObservable<bool> IsLoading { get; }
    public bool? IsLoaded { get; private set; }
    public Task WaitForLoaded(CancellationToken token = default) => throw new NotImplementedException();

    public bool AllowRecursiveDeletion => false;

    public IDeclarativeProperty<long> Size { get; }

    public ObservableCollection<ISizeScanContainer> ChildContainers { get; } = new();
    public ObservableCollection<ISizeScanElement> ChildElements { get; } = new();
    public ObservableCollection<ISizeItem> SizeItems { get; } = new();
    public required IContainer RealContainer { get; init; }

    internal SizeScanContainer()
    {
        _readOnlyExtensions = new ReadOnlyExtensionCollection(Extensions);
        IsLoading = _isLoading.AsObservable();

        _size = new(childContainerSizes => Task.FromResult(childContainerSizes.Sum()));
        Size = _size.Debounce(TimeSpan.FromSeconds(1));
        CreatedAt = DateTime.Now;
    }

    public async Task AddSizeSourceAsync(IDeclarativeProperty<long> sizeElement)
        => await _size.AddSourceAsync(sizeElement);

    public Task StartLoadingAsync()
    {
        _isLoading.OnNext(true);
        IsLoaded = false;
        return Task.CompletedTask;
    }

    public Task StopLoadingAsync()
    {
        _isLoading.OnNext(false);
        IsLoaded = true;
        return Task.CompletedTask;
    }

    public required IDeclarativeProperty<string> Status { get; init; } = new DeclarativeProperty<string>();

    public required SizeScanTask SizeScanTask { get; init; }

    public Task<ContainerEscapeResult> HandleEsc()
    {
        if (!SizeScanTask.IsRunning)
        {
            return Task.FromResult(new ContainerEscapeResult(false));
        }

        SizeScanTask.Stop();
        return Task.FromResult(new ContainerEscapeResult(true));
    }
}