using DeclarativeProperty;

namespace FileTime.App.ContainerSizeScanner;

public interface ISizePreviewItem
{
    IDeclarativeProperty<long> Size { get; }
    string Name { get; }
}