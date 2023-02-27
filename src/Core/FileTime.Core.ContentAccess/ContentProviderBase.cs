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
    private readonly IObservable<IChangeSet<AbsolutePath, string>> _items;

    protected SourceCache<AbsolutePath, string> Items { get; } = new(p => p.Path.Path);
    protected ExtensionCollection Extensions { get; }

    IObservable<IChangeSet<AbsolutePath, string>> IContainer.Items => _items;

    public string Name { get; }

    public string DisplayName { get; }

    public FullName? FullName { get; }

    public NativePath? NativePath => null;

    public bool IsHidden => false;

    public bool IsExists => true;

    public SupportsDelete CanDelete => SupportsDelete.False;

    public bool CanRename => false;

    public IContentProvider Provider => this;

    public AbsolutePath? Parent => null;

    public DateTime? CreatedAt => null;

    public string? Attributes => null;

    protected BehaviorSubject<bool> IsLoading { get; } = new(false);
    public bool AllowRecursiveDeletion => false;

    IObservable<bool> IContainer.IsLoading => IsLoading.AsObservable();

    public AbsolutePathType Type => AbsolutePathType.Container;
    public PointInTime PointInTime { get; } = PointInTime.Eternal;

    protected SourceList<Exception> Exceptions { get; } = new();
    IObservable<IChangeSet<Exception>> IItem.Exceptions => Exceptions.Connect();

    ReadOnlyExtensionCollection IItem.Extensions => _extensions;

    protected ContentProviderBase(string name)
    {
        DisplayName = Name = name;
        FullName = FullName.CreateSafe(name);
        Extensions = new ExtensionCollection();
        _extensions = Extensions.AsReadOnly();
        _items = Items.Connect().StartWithEmpty();
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

    public abstract Task<byte[]?> GetContentAsync(IElement element,
        int? maxLength = null,
        CancellationToken cancellationToken = default);

    public abstract bool CanHandlePath(NativePath path);
    public bool CanHandlePath(FullName path) => CanHandlePath(GetNativePath(path));
}