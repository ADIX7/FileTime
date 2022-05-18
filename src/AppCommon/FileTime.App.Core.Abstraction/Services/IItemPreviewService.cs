using FileTime.App.Core.ViewModels.ItemPreview;

namespace FileTime.App.Core.Services;

public interface IItemPreviewService
{
    IObservable<IItemPreviewViewModel?> ItemPreview { get; }
}