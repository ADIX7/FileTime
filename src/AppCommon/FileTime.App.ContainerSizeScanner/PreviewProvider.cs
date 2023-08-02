using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels.ItemPreview;
using FileTime.Core.Models;

namespace FileTime.App.ContainerSizeScanner;

public class PreviewProvider : IItemPreviewProvider
{
    public bool CanHandle(IItem item) => item is SizeScanContainer;

    public Task<IItemPreviewViewModel> CreatePreviewAsync(IItem item)
    {
        if (item is not SizeScanContainer container) throw new NotSupportedException();

        return Task.FromResult((IItemPreviewViewModel) new ContainerPreview(container));
    }
}