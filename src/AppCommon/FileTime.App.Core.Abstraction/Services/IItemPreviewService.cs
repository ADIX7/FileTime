using DeclarativeProperty;
using FileTime.App.Core.ViewModels.ItemPreview;

namespace FileTime.App.Core.Services;

public interface IItemPreviewService
{
    IDeclarativeProperty<IItemPreviewViewModel?> ItemPreview { get; }
}