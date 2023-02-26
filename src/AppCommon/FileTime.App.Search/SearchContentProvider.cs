using DynamicData;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.App.Search;

public class SearchContentProvider : ISearchContentProvider
{
    public string Name { get; }
    public string DisplayName { get; }
    public FullName? FullName { get; }
    public NativePath? NativePath { get; }
    public AbsolutePath? Parent { get; }
    public bool IsHidden { get; }
    public bool IsExists { get; }
    public DateTime? CreatedAt { get; }
    public SupportsDelete CanDelete { get; }
    public bool CanRename { get; }
    public IContentProvider Provider { get; }
    public string? Attributes { get; }
    public AbsolutePathType Type { get; }
    public PointInTime PointInTime { get; }
    public IObservable<IChangeSet<Exception>> Exceptions { get; }
    public ReadOnlyExtensionCollection Extensions { get; }
    public IObservable<IObservable<IChangeSet<AbsolutePath, string>>?> Items { get; }
    public IObservable<bool> IsLoading { get; }
    public bool AllowRecursiveDeletion { get; }
    public Task OnEnter() => throw new NotImplementedException();

    public bool SupportsContentStreams { get; }
    public Task<IItem> GetItemByFullNameAsync(FullName fullName, PointInTime pointInTime, bool forceResolve = false, AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown, ItemInitializationSettings itemInitializationSettings = default) => throw new NotImplementedException();

    public Task<IItem> GetItemByNativePathAsync(NativePath nativePath, PointInTime pointInTime, bool forceResolve = false, AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown, ItemInitializationSettings itemInitializationSettings = default) => throw new NotImplementedException();

    public NativePath GetNativePath(FullName fullName) => throw new NotImplementedException();

    public Task<byte[]?> GetContentAsync(IElement element, int? maxLength = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public bool CanHandlePath(NativePath path) => throw new NotImplementedException();

    public bool CanHandlePath(FullName path) => throw new NotImplementedException();
}