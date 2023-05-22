using System.Runtime.InteropServices;
using DynamicData;
using DynamicData.Binding;
using FileTime.App.Core.Services;
using FileTime.Core.Extensions;
using FileTime.Core.Models;
using FileTime.GuiApp.ViewModels;
using FileTime.Providers.Local;

namespace FileTime.GuiApp.Services;

public class RootDriveInfoService : IStartupHandler
{
    private readonly List<DriveInfo> _rootDrives = new();

    public RootDriveInfoService(
        IGuiAppState guiAppState,
        ILocalContentProvider localContentProvider)
    {
        InitRootDrives();

        var rootDriveInfos = localContentProvider.Items.Transform(
            i =>
            {
                var rootDrive = _rootDrives.FirstOrDefault(d =>
                {
                    var containerPath = localContentProvider.GetNativePath(i.Path).Path;
                    var drivePath = d.Name.TrimEnd(Path.DirectorySeparatorChar);
                    return containerPath == drivePath
                           || (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && containerPath == "/" &&
                               d.Name == "/");
                });

                return (Path: i, Drive: rootDrive);
            }
        )
        .Filter(t => t.Drive is not null)
        .TransformAsync(async t => (Item: await t.Path.ResolveAsyncSafe(), Drive: t.Drive!))
        .Filter(t => t.Item is IContainer)
        .Transform(t => (Container: (IContainer) t.Item!, t.Drive))
        .Transform(t => new RootDriveInfo(t.Drive, t.Container))
        .Sort(SortExpressionComparer<RootDriveInfo>.Ascending(d => d.Name));

        guiAppState.RootDriveInfos = rootDriveInfos.ToBindedCollection();

        void InitRootDrives()
        {
            var driveInfos = new List<RootDriveInfo>();
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

    public Task InitAsync() => Task.CompletedTask;
}