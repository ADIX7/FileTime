using System.Collections.ObjectModel;
using FileTime.Core.Interactions;

namespace FileTime.App.Core.Interactions;

public class PreviewList : IPreviewElement
{
    public ObservableCollection<IPreviewElement> Items { get; } = new();
    public PreviewType PreviewType { get; } = PreviewType.PreviewList;
    object IPreviewElement.PreviewType => PreviewType;
}