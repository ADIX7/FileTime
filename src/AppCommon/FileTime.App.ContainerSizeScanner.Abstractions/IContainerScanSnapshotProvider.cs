using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.App.ContainerSizeScanner;

public interface IContainerScanSnapshotProvider : IContentProvider
{
    ISizeScanTask StartSizeScan(IContainer scanSizeOf);
}