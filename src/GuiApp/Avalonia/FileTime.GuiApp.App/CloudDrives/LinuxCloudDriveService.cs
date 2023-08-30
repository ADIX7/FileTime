using PropertyChanged.SourceGenerator;

namespace FileTime.GuiApp.App.CloudDrives;

public partial class LinuxCloudDriveService : ICloudDriveService
{
    [Notify] private IReadOnlyList<CloudDrive> _cloudDrives = new List<CloudDrive>();
    
    public Task InitAsync() => Task.CompletedTask;
}