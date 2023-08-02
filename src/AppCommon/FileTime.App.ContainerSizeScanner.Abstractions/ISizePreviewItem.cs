using DeclarativeProperty;
using FileTime.App.Core.ViewModels.ItemPreview;

namespace FileTime.App.ContainerSizeScanner;

public interface ISizePreviewItem
{
    IDeclarativeProperty<long> Size { get; }
    string Name { get; }
}