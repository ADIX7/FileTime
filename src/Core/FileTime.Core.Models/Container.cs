using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Timeline;

namespace FileTime.Core.Models;

public record Container(
    string Name,
    string DisplayName,
    FullName FullName,
    NativePath NativePath,
    AbsolutePath? Parent,
    bool IsHidden,
    bool IsExists,
    DateTime? CreatedAt,
    SupportsDelete CanDelete,
    bool CanRename,
    string? Attributes,
    IContentProvider Provider,
    bool AllowRecursiveDeletion,
    PointInTime PointInTime,
    IObservable<IEnumerable<Exception>> Exceptions,
    ReadOnlyExtensionCollection Extensions,
    IObservable<IObservable<IChangeSet<AbsolutePath, string>>?> Items) : IContainer
{
    BehaviorSubject<bool> IsLoading { get; } = new(false);
    IObservable<bool> IContainer.IsLoading => IsLoading.AsObservable();
    public AbsolutePathType Type => AbsolutePathType.Container;
}