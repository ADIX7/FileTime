using System.Collections.ObjectModel;
using FileTime.Core.Enums;
using FileTime.Core.Services;
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
    IObservable<IEnumerable<Exception>> Exceptions,
    ReadOnlyExtensionCollection Extensions) : IElement
{
    public AbsolutePathType Type => AbsolutePathType.Element;
}