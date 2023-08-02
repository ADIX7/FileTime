using DeclarativeProperty;

namespace FileTime.App.ContainerSizeScanner;

public class ChildElementPreview : ISizePreviewItem
{
    public ChildElementPreview(ISizeScanElement element)
    {
        Name = element.Name;
        Size = element.Size;
    }

    public string Name { get; }
    public IDeclarativeProperty<long> Size { get; }
}