using System.Collections.ObjectModel;
using FileTime.Core.Interactions;

namespace FileTime.App.Core.Interactions;

public class DoubleTextListPreview : IPreviewElement
{
    public ObservableCollection<DoubleTextPreview> Items { get; } = new();
    public PreviewType PreviewType { get; } = PreviewType.DoubleTextList;
    object IPreviewElement.PreviewType => PreviewType;
}