using System.Collections.ObjectModel;
using DeclarativeProperty;
using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Timeline;

namespace FileTime.App.ContainerSizeScanner;

public record SizeScanElement : ISizeScanElement
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public required FullName FullName { get; init; }
    public required NativePath NativePath { get; init; }
    public required AbsolutePath? Parent { get; init; }
    public required DateTime? CreatedAt { get; init; }
    public required DateTime? ModifiedAt { get; init;}

    public required IDeclarativeProperty<long> Size { get; init; }
    public bool IsHidden => false;
    public bool IsExists => true;
    public SupportsDelete CanDelete => SupportsDelete.False;
    public bool CanRename => false;
    public string? Attributes => "";
    public required IContentProvider Provider { get; init; }
    public PointInTime PointInTime { get; } = PointInTime.Present;
    public ObservableCollection<Exception> Exceptions { get; } = new();
    public ReadOnlyExtensionCollection Extensions { get; } = new();
    public AbsolutePathType Type => AbsolutePathType.Element;

    public IItem WithParent(AbsolutePath parent) => this with {Parent = parent};
}