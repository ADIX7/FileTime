using FileTime.App.Core.Services;

namespace FileTime.GuiApp.App.CloudDrives;

public interface ICloudDriveService : IStartupHandler
{
    IReadOnlyList<CloudDrive> CloudDrives { get; }
}