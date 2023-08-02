using DeclarativeProperty;

namespace FileTime.App.ContainerSizeScanner;

public class ChildContainerPreview : ISizePreviewItem
{
    public ChildContainerPreview(ISizeScanContainer container)
    {
        Name = container.Name;
        Size = container.Size;
    }

    public string Name { get; }
    public IDeclarativeProperty<long> Size { get; }
}