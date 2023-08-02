using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DeclarativeProperty;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.App.ContainerSizeScanner;

//TODO: create readonly version
public class ContainerSizeScanContainer : IContainerSizeScanContainer
{
    private readonly ReadOnlyExtensionCollection _readOnlyExtensions;
    private readonly BehaviorSubject<bool> _isLoading = new(false);
    private readonly CombineProperty<long, long> _size;
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public required FullName? FullName { get; init; }
    public required NativePath? NativePath { get; init; }
    public AbsolutePath? Parent { get; init; }
    public bool IsHidden => false;
    public bool IsExists => true;
    public DateTime? CreatedAt { get; }
    public SupportsDelete CanDelete => SupportsDelete.True;
    public bool CanRename => false;
    public IContentProvider Provider { get; }
    public string? Attributes => null;
    public AbsolutePathType Type => AbsolutePathType.Container;
    public PointInTime PointInTime => PointInTime.Present;
    public ObservableCollection<Exception> Exceptions { get; } = new();
    public ExtensionCollection Extensions { get; } = new();
    ReadOnlyExtensionCollection IItem.Extensions => _readOnlyExtensions;
    public IItem WithParent(AbsolutePath parent) => throw new NotImplementedException();

    public ObservableCollection<AbsolutePath> Items { get; } = new();
    public IObservable<bool> IsLoading { get; }
    public bool? IsLoaded { get; private set; }
    public Task WaitForLoaded(CancellationToken token = default) => throw new NotImplementedException();

    public bool AllowRecursiveDeletion => false;

    public IDeclarativeProperty<long> Size => _size;
    public ObservableCollection<IContainerSizeScanContainer> ChildContainers { get; } = new();
    public required IContainer RealContainer { get; init; }

    public ContainerSizeScanContainer(IContainerScanSnapshotProvider provider)
    {
        _readOnlyExtensions = new ReadOnlyExtensionCollection(Extensions);
        IsLoading = _isLoading.AsObservable();

        _size = new(childContainerSizes => Task.FromResult(childContainerSizes.Sum()));
        CreatedAt = DateTime.Now;
        Provider = provider;
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
}