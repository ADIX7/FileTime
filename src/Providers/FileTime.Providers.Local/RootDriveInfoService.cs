using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using FileTime.App.Core.Services;
using FileTime.Core.Models;
using ObservableComputations;

namespace FileTime.Providers.Local;

public class RootDriveInfoService : IRootDriveInfoService, IExitHandler
{
    private readonly ILocalContentProvider _localContentProvider;
    private readonly List<DriveInfo> _rootDrives = new();
    private readonly OcConsumer _rootDriveInfosConsumer = new();

    public ObservableCollection<RootDriveInfo> RootDriveInfos { get; set; }

    public RootDriveInfoService(ILocalContentProvider localContentProvider)
    {
        _localContentProvider = localContentProvider;
        InitRootDrives();

        var rootDriveInfos = localContentProvider.Items.Selecting<AbsolutePath, (AbsolutePath Path, DriveInfo? Drive)>(
                i => MatchRootDrive(i)
            )
            .Filtering(t => IsNotNull(t.Drive))
            .Selecting(t => Resolve(t))
            .Filtering(t => t.Item is IContainer)
            .Selecting(t => new RootDriveInfo(t.Drive, (IContainer) t.Item!))
            .Ordering(d => d.Name);

        rootDriveInfos.For(_rootDriveInfosConsumer);

        RootDriveInfos = rootDriveInfos;

        void InitRootDrives()
        {
            var drives = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed)
                : DriveInfo.GetDrives().Where(d =>
                    d.DriveType == DriveType.Fixed
                    && d.DriveFormat != "pstorefs"
                    && d.DriveFormat != "bpf_fs"
                    && d.DriveFormat != "tracefs"
                    && !d.RootDirectory.FullName.StartsWith("/snap/"));

            _rootDrives.Clear();
            _rootDrives.AddRange(drives);
        }
    }

    private static bool IsNotNull(object? obj) => obj is not null;

    private static (IItem? Item, DriveInfo Drive) Resolve((AbsolutePath Path, DriveInfo? Drive) tuple)
    {
        var t = Task.Run(async () => await tuple.Path.ResolveAsyncSafe());
        t.Wait();
        return (Item: t.Result, Drive: tuple.Drive!);
    }

    private (AbsolutePath Path, DriveInfo? Drive) MatchRootDrive(AbsolutePath sourceItem)
    {
        var rootDrive = _rootDrives.FirstOrDefault(d =>
        {
            var containerPath = _localContentProvider.GetNativePath(sourceItem.Path).Path;
            var drivePath = d.Name.TrimEnd(Path.DirectorySeparatorChar);
            return containerPath == drivePath
                   || (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && containerPath == "/" &&
                       d.Name == "/");
        });

        return (Path: sourceItem, Drive: rootDrive);
    }

    public Task ExitAsync(CancellationToken token = default)
    {
        _rootDriveInfosConsumer.Dispose();
        return Task.CompletedTask;
    }
}