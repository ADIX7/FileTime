using DeclarativeProperty;
using FileTime.App.Core.ViewModels;
using FileTime.App.Core.ViewModels.ItemPreview;

namespace FileTime.App.Core.Services;

public class ItemPreviewService : IItemPreviewService
{
    private readonly IEnumerable<IItemPreviewProvider> _itemPreviewProviders;
    public IDeclarativeProperty<IItemPreviewViewModel?> ItemPreview { get; }

    public ItemPreviewService(
        IAppState appState,
        IEnumerable<IItemPreviewProvider> itemPreviewProviders)
    {
        _itemPreviewProviders = itemPreviewProviders;
        ItemPreview = appState
            .SelectedTab
            .Map(t => t?.CurrentSelectedItem)
            .Switch()
            .Debounce(TimeSpan.FromMilliseconds(250))
            .Map(async (item, _) =>
                item == null
                    ? null
                    : await Map(item)
            )
            .DistinctUntilChanged();
    }

    private async Task<IItemPreviewViewModel?> Map(IItemViewModel itemViewModel)
    {
        ArgumentNullException.ThrowIfNull(itemViewModel.BaseItem);

        var itemPreviewProvider = _itemPreviewProviders.FirstOrDefault(p => p.CanHandle(itemViewModel.BaseItem));

        return itemPreviewProvider is null
            ? null
            : await itemPreviewProvider.CreatePreviewAsync(itemViewModel.BaseItem);
    }
}