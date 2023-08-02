using FileTime.Core.Models;
using InitableService;

namespace FileTime.App.ContainerSizeScanner;

public interface ISizeScanTask : IInitable<IContainer>
{
    IContainerSizeScanContainer SizeContainer { get; }
    void Start();
}