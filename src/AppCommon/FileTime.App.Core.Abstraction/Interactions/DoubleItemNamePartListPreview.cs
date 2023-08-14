using FileTime.Core.Interactions;
using FileTime.Core.Models;
using PropertyChanged.SourceGenerator;

namespace FileTime.App.Core.Interactions;

public partial class DoubleItemNamePartListPreview : IPreviewElement
{
    [Notify] private List<ItemNamePart> _itemNameParts1 = new();
    [Notify] private List<ItemNamePart> _itemNameParts2 = new();
    public PreviewType PreviewType => PreviewType.DoubleItemNamePartList;
    object IPreviewElement.PreviewType => PreviewType;
}