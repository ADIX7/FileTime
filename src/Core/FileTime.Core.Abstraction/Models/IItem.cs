using System.Collections.ObjectModel;
using DynamicData;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Timeline;

namespace FileTime.Core.Models;

public interface IItem
{
    string Name { get; }
    string DisplayName { get; }
    FullName? FullName { get; }
    NativePath? NativePath { get; }
    AbsolutePath? Parent { get; }
    bool IsHidden { get; }
    bool IsExists { get; }
    DateTime? CreatedAt { get; }
    SupportsDelete CanDelete { get; }
    bool CanRename { get; }
    IContentProvider Provider { get; }
    string? Attributes { get; }
    AbsolutePathType Type { get; }
    PointInTime PointInTime { get; }
    ObservableCollection<Exception> Exceptions { get; }
    ReadOnlyExtensionCollection Extensions { get; }

    T? GetExtension<T>() => (T?)Extensions.FirstOrDefault(i => i is T);
}
