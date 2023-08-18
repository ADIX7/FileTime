using FileTime.App.Core.Models;

namespace FileTime.App.Core.ViewModels.ItemPreview;

public interface IElementPreviewViewModel : IItemPreviewViewModel
{
    ItemPreviewMode Mode { get; }
    string TextContent { get; }
    string TextEncoding { get; }
}