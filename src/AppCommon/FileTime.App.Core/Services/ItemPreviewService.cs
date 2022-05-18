using System.Reactive.Linq;
using FileTime.App.Core.ViewModels;
using FileTime.App.Core.ViewModels.ItemPreview;
using FileTime.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.App.Core.Services;

public class ItemPreviewService : IItemPreviewService
{
    private readonly IServiceProvider _serviceProvider;
    public IObservable<IItemPreviewViewModel?> ItemPreview { get; }

    public ItemPreviewService(IAppState appState, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        ItemPreview = appState
            .SelectedTab
            .Select(t => t?.CurrentSelectedItem.Throttle(TimeSpan.FromMilliseconds(250)) ?? Observable.Return<IItemViewModel?>(null))
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
        return itemViewModel.BaseItem switch
        {
            IElement element => await _serviceProvider.GetAsyncInitableResolver(element)
                .GetRequiredServiceAsync<ElementPreviewViewModel>(),
            _ => null
        };
    }
}