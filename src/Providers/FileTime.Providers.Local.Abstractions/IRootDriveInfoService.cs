using System.Collections.ObjectModel;
using FileTime.App.Core.Services;

namespace FileTime.Providers.Local;

public interface IRootDriveInfoService : IExitHandler
{
    ReadOnlyObservableCollection<RootDriveInfo> RootDriveInfos { get; set; }
    ReadOnlyObservableCollection<DriveInfo> AllDrives { get; set; }
}