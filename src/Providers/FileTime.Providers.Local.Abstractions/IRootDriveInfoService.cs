using System.Collections.ObjectModel;
using FileTime.App.Core.Services;

namespace FileTime.Providers.Local;

public interface IRootDriveInfoService : IExitHandler
{
    ObservableCollection<RootDriveInfo> RootDriveInfos { get; set; }
}