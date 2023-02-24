using DynamicData;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Timeline;

namespace FileTime.Core.Models;

public record Element(
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
    PointInTime PointInTime,
    IObservable<IChangeSet<Exception>> Exceptions,
    ReadOnlyExtensionCollection Extensions) : IElement
{
    public AbsolutePathType Type => AbsolutePathType.Element;
}