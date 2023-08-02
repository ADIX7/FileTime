using System.Collections.ObjectModel;
using System.ComponentModel;
using FileTime.App.Core.ViewModels.ItemPreview;
using ObservableComputations;

namespace FileTime.App.ContainerSizeScanner;

public class ContainerPreview : IItemPreviewViewModel, IDisposable
{
    private readonly OcConsumer _topItemsConsumer = new();
    public const string PreviewName = "SizePreviewContainer";
    public string Name => PreviewName;

    public ObservableCollection<ISizePreviewItem> TopItems { get; }

    public ContainerPreview(ISizeScanContainer sizeScanContainer)
    {
        TopItems = sizeScanContainer
            .SizeItems
            .Ordering(c => c.Size.Value, ListSortDirection.Descending)
            .Taking(0, 10)
            .Selecting(i => CreatePreviewItem(i))
            .For(_topItemsConsumer);
    }

    private ISizePreviewItem CreatePreviewItem(ISizeItem sizeItem)
        => sizeItem switch
            {
                ISizeScanContainer container => new ChildContainerPreview(container),
                ISizeScanElement element => new ChildElementPreview(element),
                _ => throw new ArgumentOutOfRangeException(nameof(sizeItem))
            };

    public void Dispose() 
        => _topItemsConsumer.Dispose();
}