using System.Reactive.Linq;
using FileTime.App.Core.ViewModels;
using FileTime.App.Core.ViewModels.ItemPreview;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.App.Core.Services;

public class ItemPreviewService : IItemPreviewService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IItemPreviewProvider> _itemPreviewProviders;
    public IObservable<IItemPreviewViewModel?> ItemPreview { get; }

    public ItemPreviewService(
        IAppState appState,
        IServiceProvider serviceProvider,
        IEnumerable<IItemPreviewProvider> itemPreviewProviders)
    {
        _serviceProvider = serviceProvider;
        _itemPreviewProviders = itemPreviewProviders;
        ItemPreview = appState
            .SelectedTab
            .Select(t =>
                t?.CurrentSelectedItem.Throttle(TimeSpan.FromMilliseconds(250))
                ?? Observable.Return<IItemViewModel?>(null))
            .Switch()
            .Select(item =>
                item == null
                    ? Observable.Return<IItemPreviewViewModel?>(null)
                    : Observable.FromAsync(async () => await Map(item))
            )
            .Switch()
            .Publish(null)
            .RefCount();
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