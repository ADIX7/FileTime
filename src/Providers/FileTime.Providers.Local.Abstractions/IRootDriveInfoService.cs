using System.Collections.ObjectModel;

namespace FileTime.Providers.Local;

public interface IRootDriveInfoService
{
    ObservableCollection<RootDriveInfo> RootDriveInfos { get; set; }
}