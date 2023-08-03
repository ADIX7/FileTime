using FileTime.App.Core.Services;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;

namespace FileTime.App.ContainerSizeScanner;

public interface IContainerSizeScanProvider : IContentProvider, IExitHandler
{
    ISizeScanTask StartSizeScan(IContainer scanSizeOf);
}