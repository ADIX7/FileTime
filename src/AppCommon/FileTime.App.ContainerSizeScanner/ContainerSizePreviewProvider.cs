using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels.ItemPreview;
using FileTime.Core.Models;

namespace FileTime.App.ContainerSizeScanner;

public class ContainerSizePreviewProvider : IItemPreviewProvider
{
    public bool CanHandle(IItem item) => item is ContainerSizeScanContainer;

    public Task<IItemPreviewViewModel> CreatePreviewAsync(IItem item)
    {
        if(item is not ContainerSizeScanContainer container) throw new NotSupportedException();

        return Task.FromResult((IItemPreviewViewModel)new ContainerSizeContainerPreview(container));
    }
}