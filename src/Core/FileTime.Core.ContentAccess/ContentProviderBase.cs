using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.Core.ContentAccess;

public abstract class ContentProviderBase : IContentProvider
{
    private readonly ReadOnlyExtensionCollection _extensions;
    public ObservableCollection<AbsolutePath> Items { get; }
    protected ExtensionCollection Extensions { get; }

    public string Name { get; }

    public string DisplayName { get; }

    public FullName? FullName { get; }

    public NativePath? NativePath => null;

    public bool IsHidden => false;

    public bool IsExists => true;

    public SupportsDelete CanDelete => SupportsDelete.False;

    public bool CanRename => false;

    public IContentProvider Provider => this;

    public AbsolutePath? Parent { get; }

    public DateTime? CreatedAt => null;
    public DateTime? ModifiedAt => null;

    public string? Attributes => null;

    protected BehaviorSubject<bool> IsLoading { get; } = new(false);
    IObservable<bool> IContainer.IsLoading => IsLoading.AsObservable();
    public bool? IsLoaded => true;
    public Task WaitForLoaded(CancellationToken token = default) => Task.CompletedTask;

    public bool AllowRecursiveDeletion => false;

    public AbsolutePathType Type => AbsolutePathType.Container;
    public PointInTime PointInTime => PointInTime.Eternal;

    public ObservableCollection<Exception> Exceptions { get; } = new();

    ReadOnlyExtensionCollection IItem.Extensions => _extensions;

    protected ContentProviderBase(string name, ITimelessContentProvider timelessContentProvider)
    {
        Parent = new AbsolutePath(timelessContentProvider, PointInTime.Eternal, new FullName(""), AbsolutePathType.Container);
        DisplayName = Name = name;
        FullName = FullName.CreateSafe(name);
        Extensions = new ExtensionCollection();
        _extensions = Extensions.AsReadOnly();
        //TODO: 
        Items = new ObservableCollection<AbsolutePath>();
    }

    public virtual Task OnEnter() => Task.CompletedTask;

    public virtual bool SupportsContentStreams { get; protected set; }

    public virtual async Task<IItem> GetItemByFullNameAsync(FullName fullName,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default)
        => await GetItemByNativePathAsync(await GetNativePathAsync(fullName), pointInTime, forceResolve, forceResolvePathType,
            itemInitializationSettings);

    public abstract Task<IItem> GetItemByNativePathAsync(NativePath nativePath,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default);

    public abstract ValueTask<NativePath> GetNativePathAsync(FullName fullName);
    public abstract FullName GetFullName(NativePath nativePath);

    public abstract Task<byte[]?> GetContentAsync(IElement element,
        int? maxLength = null,
        CancellationToken cancellationToken = default);

    public abstract Task<bool> CanHandlePathAsync(NativePath path);
    public async Task<bool> CanHandlePathAsync(FullName path) 
        => path.Path.TrimEnd(Constants.SeparatorChar) == Name 
           || await CanHandlePathAsync(await GetNativePathAsync(path));

    public abstract VolumeSizeInfo? GetVolumeSizeInfo(FullName path);

    public IItem WithParent(AbsolutePath parent) => this; 
}