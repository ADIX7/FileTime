using DeclarativeProperty;
using FileTime.Core.Models;

namespace FileTime.App.ContainerSizeScanner;

public class ChildElementPreview : ISizePreviewItem
{
    public ChildElementPreview(ISizeScanElement element)
    {
        Name = element.Name;
        Size = new DeclarativeProperty<long>(((IElement) element).Size);
    }

    public string Name { get; }
    public IDeclarativeProperty<long> Size { get; }
}