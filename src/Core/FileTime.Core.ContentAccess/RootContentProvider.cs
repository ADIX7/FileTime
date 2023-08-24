using System.Collections.ObjectModel;
using System.Reactive.Linq;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using ObservableComputations;

namespace FileTime.Core.ContentAccess;

public class RootContentProvider : IRootContentProvider
{
    private readonly ITimelessContentProvider _timelessContentProvider;
    public string Name => "root";
    public string DisplayName => "Root";
    public FullName? FullName => null;
    public NativePath? NativePath => null;
    public AbsolutePath? Parent => null;
    public bool IsHidden => false;
    public bool IsExists => true;
    public DateTime? CreatedAt => null;
    public DateTime? ModifiedAt => null;
    public SupportsDelete CanDelete => SupportsDelete.False;
    public bool CanRename => false;
    public IContentProvider Provider => this;
    public string? Attributes => null;
    public AbsolutePathType Type => AbsolutePathType.Container;
    public PointInTime PointInTime => PointInTime.Eternal;
    public ObservableCollection<Exception> Exceptions { get; } = new();
    public ReadOnlyExtensionCollection Extensions { get; } = new(new ExtensionCollection());
    public ObservableCollection<AbsolutePath> Items { get; }
    public IObservable<bool> IsLoading => Observable.Return(false);
    public bool? IsLoaded => true;
    public Task WaitForLoaded(CancellationToken token = default) => Task.CompletedTask;

    public bool AllowRecursiveDeletion => false;
    public Task OnEnter() => Task.CompletedTask;

    public bool SupportsContentStreams => false;

    public RootContentProvider(
        IContentProviderRegistry contentProviderRegistry,
        ITimelessContentProvider timelessContentProvider)
    {
        _timelessContentProvider = timelessContentProvider;
        Items = contentProviderRegistry
            .ContentProviders
            .Selecting<IContentProvider, AbsolutePath>(c =>
                new AbsolutePath(timelessContentProvider, c)
            );
    }

    public async Task<IItem> GetItemByFullNameAsync(
        FullName fullName,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default)
        => await _timelessContentProvider.GetItemByFullNameAsync(
            fullName,
            pointInTime,
            forceResolve,
            forceResolvePathType,
            itemInitializationSettings);

    public async Task<IItem> GetItemByNativePathAsync(
        NativePath nativePath,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default)
        => await _timelessContentProvider.GetItemByNativePathAsync(
            nativePath,
            pointInTime
        ) ?? throw new FileNotFoundException();

    public ValueTask<NativePath> GetNativePathAsync(FullName fullName) => throw new NotImplementedException();

    public FullName GetFullName(NativePath nativePath) => throw new NotImplementedException();

    public Task<byte[]?> GetContentAsync(
        IElement element,
        int? maxLength = null,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<bool> CanHandlePathAsync(NativePath path) => throw new NotImplementedException();

    public Task<bool> CanHandlePathAsync(FullName path) => throw new NotImplementedException();
    public VolumeSizeInfo? GetVolumeSizeInfo(FullName path) => null;

    public IItem WithParent(AbsolutePath parent) => this;
}