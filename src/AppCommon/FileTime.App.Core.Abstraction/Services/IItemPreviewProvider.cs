using FileTime.App.Core.ViewModels.ItemPreview;
using FileTime.Core.Models;

namespace FileTime.App.Core.Services;

public interface IItemPreviewProvider
{
    bool CanHandle(IItem item);
    Task<IItemPreviewViewModel> CreatePreviewAsync(IItem item);
}