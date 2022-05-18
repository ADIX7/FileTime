using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using FileTime.Core.Enums;
using FileTime.Core.Services;

namespace FileTime.Core.Models;

public record Container(
    string Name,
    string DisplayName,
    FullName FullName,
    NativePath NativePath,
    IAbsolutePath? Parent,
    bool IsHidden,
    bool IsExists,
    DateTime? CreatedAt,
    SupportsDelete CanDelete,
    bool CanRename,
    string? Attributes,
    IContentProvider Provider,
    IObservable<IEnumerable<Exception>> Exceptions,
    ReadOnlyExtensionCollection Extensions,
    IObservable<IObservable<IChangeSet<IAbsolutePath>>?> Items) : IContainer
{
    BehaviorSubject<bool> IsLoading { get; } = new BehaviorSubject<bool>(false);
    IObservable<bool> IContainer.IsLoading => IsLoading.AsObservable();
    public AbsolutePathType Type => AbsolutePathType.Container;
}