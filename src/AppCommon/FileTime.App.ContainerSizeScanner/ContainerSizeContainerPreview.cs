using System.Collections.ObjectModel;
using FileTime.App.Core.ViewModels.ItemPreview;
using ObservableComputations;

namespace FileTime.App.ContainerSizeScanner;

public class ContainerSizeContainerPreview : ISizePreviewItem
{
    public const string PreviewName = "SizePreviewContainer";
    public string Name => PreviewName;

    public ObservableCollection<ISizePreviewItem> Items { get; }

    public ContainerSizeContainerPreview(IContainerSizeScanContainer container)
    {
        Items = container
            .ChildContainers
            .Ordering(c => c.Size)
            .Taking(0, 10)
            .Selecting();
    }
}