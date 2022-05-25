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

    protected BehaviorSubject<IObservable<IChangeSet<AbsolutePath, string>>?> Items { get; } = new(null);
    protected ExtensionCollection Extensions { get; }

    IObservable<IObservable<IChangeSet<AbsolutePath, string>>?> IContainer.Items => Items;

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

    IObservable<bool> IContainer.IsLoading => IsLoading.AsObservable();

    public AbsolutePathType Type => AbsolutePathType.Container;
    public PointInTime PointInTime { get; } = PointInTime.Eternal;

    public IObservable<IEnumerable<Exception>> Exceptions => Observable.Return(Enumerable.Empty<Exception>());

    ReadOnlyExtensionCollection IItem.Extensions => _extensions;

    protected ContentProviderBase(string name)
    {
        DisplayName = Name = name;
        FullName = new FullName(name);
        Extensions = new ExtensionCollection();
        _extensions = Extensions.AsReadOnly();
    }

    public virtual Task OnEnter() => Task.CompletedTask;

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

    public abstract Task<List<AbsolutePath>> GetItemsByContainerAsync(FullName fullName, PointInTime pointInTime);
    public abstract NativePath GetNativePath(FullName fullName);

    public abstract Task<byte[]?> GetContentAsync(IElement element,
        int? maxLength = null,
        CancellationToken cancellationToken = default);
}