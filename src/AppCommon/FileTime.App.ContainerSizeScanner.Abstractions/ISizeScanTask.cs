using FileTime.Core.Models;
using InitableService;

namespace FileTime.App.ContainerSizeScanner;

public interface ISizeScanTask : IInitable<IContainer>
{
    ISizeScanContainer SizeSizeScanContainer { get; }
    bool IsRunning { get; }
    void Start();
    void Stop();
}