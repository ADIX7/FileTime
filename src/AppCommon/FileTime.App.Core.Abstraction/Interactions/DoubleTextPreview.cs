using FileTime.Core.Interactions;
using PropertyChanged.SourceGenerator;

namespace FileTime.App.Core.Interactions;

public partial class DoubleTextPreview : IPreviewElement
{
    [Notify] private string _text1;
    [Notify] private string _text2;

    public PreviewType PreviewType => PreviewType.DoubleText;
    object IPreviewElement.PreviewType => PreviewType;
}