using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
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
        => await GetItemByNativePathAsync(GetNativePath(fullName), pointInTime, forceResolve, forceResolvePathType,
            itemInitializationSettings);

    public abstract Task<IItem> GetItemByNativePathAsync(NativePath nativePath,
        PointInTime pointInTime,
        bool forceResolve = false,
        AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown,
        ItemInitializationSettings itemInitializationSettings = default);

    public abstract NativePath GetNativePath(FullName fullName);
    public abstract FullName GetFullName(NativePath nativePath);

    public abstract Task<byte[]?> GetContentAsync(IElement element,
        int? maxLength = null,
        CancellationToken cancellationToken = default);

    public abstract bool CanHandlePath(NativePath path);
    public bool CanHandlePath(FullName path) => CanHandlePath(GetNativePath(path));
    public IItem WithParent(AbsolutePath parent) => this; 
}