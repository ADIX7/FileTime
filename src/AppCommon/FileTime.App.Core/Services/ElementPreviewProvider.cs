using FileTime.App.Core.ViewModels.ItemPreview;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.App.Core.Services;

public class ElementPreviewProvider : IItemPreviewProvider
{
    private readonly IServiceProvider _serviceProvider;

    public ElementPreviewProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool CanHandle(IItem item) => item is IElement;

    public async Task<IItemPreviewViewModel> CreatePreviewAsync(IItem item)
    {
        if (item is not IElement element) throw new NotSupportedException();

        return await _serviceProvider
            .GetAsyncInitableResolver(element)
            .GetRequiredServiceAsync<ElementPreviewViewModel>();
    }
}