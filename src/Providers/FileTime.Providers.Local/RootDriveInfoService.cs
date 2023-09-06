using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using ObservableComputations;

namespace FileTime.Providers.Local;

public class RootDriveInfoService : IRootDriveInfoService
{
    private readonly ILocalContentProvider _localContentProvider;
    private readonly ObservableCollection<DriveInfo> _rootDrives = new();
    private readonly ObservableCollection<DriveInfo> _allDrives = new();
    private readonly OcConsumer _rootDriveInfosConsumer = new();

    public ReadOnlyObservableCollection<DriveInfo> AllDrives { get; set; }
    public ReadOnlyObservableCollection<RootDriveInfo> RootDriveInfos { get; set; }

    public RootDriveInfoService(ILocalContentProvider localContentProvider)
    {
        _localContentProvider = localContentProvider;
        var (rootDrives, allDrives) = GetRootDrives();

        foreach (var driveInfo in rootDrives)
        {
            _rootDrives.Add(driveInfo);
        }
        
        foreach (var driveInfo in allDrives)
        {
            _allDrives.Add(driveInfo);
        }

        var rootDriveInfos = _rootDrives.Selecting(r => GetContainer(r))
            .Filtering(t => t.Item != null)
            .Selecting(t => new RootDriveInfo(t.Drive, t.Item!))
            .Ordering(d => GetDriveOrder(d.DriveType))
            .ThenOrdering(d => d.Name)
            .For(_rootDriveInfosConsumer);

        RootDriveInfos = new ReadOnlyObservableCollection<RootDriveInfo>(rootDriveInfos);
        AllDrives = new ReadOnlyObservableCollection<DriveInfo>(_allDrives);

        static (DriveInfo[] RootDrives, DriveInfo[] AllDrives) GetRootDrives()
        {
            var allDrives = DriveInfo.GetDrives();
            var drives = DriveInfo.GetDrives().Where(d => d.DriveType is not DriveType.Unknown and not DriveType.Ram);
            drives = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? drives
                : drives.Where(d =>
                    d.TotalSize != 0
                    && d.DriveFormat != "pstorefs"
                    && d.DriveFormat != "bpf_fs"
                    && d.DriveFormat != "tracefs"
                    && d.DriveFormat != "rpc_pipefs"
                    && !d.RootDirectory.FullName.StartsWith("/snap/"));

            return (drives.ToArray(), allDrives);
        }
    }

    private (DriveInfo Drive, IContainer? Item) GetContainer(DriveInfo rootDriveInfo)
    {
        var task = Task.Run(
            async () => await _localContentProvider.GetItemByNativePathAsync(
                new NativePath(rootDriveInfo.RootDirectory.FullName),
                PointInTime.Present)
        );
        task.Wait();

        return (rootDriveInfo, task.Result as IContainer);
    }

    private static int GetDriveOrder(DriveType type) 
        => type switch
        {
            DriveType.Fixed => 0,
            DriveType.Removable => 1,
            DriveType.CDRom => 2,
            DriveType.Network => 3,
            _ => 5
        };

    public Task ExitAsync(CancellationToken token = default)
    {
        _rootDriveInfosConsumer.Dispose();
        return Task.CompletedTask;
    }
}