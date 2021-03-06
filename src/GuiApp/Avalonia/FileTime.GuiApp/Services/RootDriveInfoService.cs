using System.Reactive.Linq;
using System.Runtime.InteropServices;
using DynamicData;
using DynamicData.Binding;
using FileTime.App.Core.Models;
using FileTime.App.Core.Services;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using FileTime.GuiApp.ViewModels;
using FileTime.Providers.Local;

namespace FileTime.GuiApp.Services;

public class RootDriveInfoService : IStartupHandler
{
    private readonly SourceList<DriveInfo> _rootDrives = new();

    public RootDriveInfoService(
        IGuiAppState guiAppState, 
        ILocalContentProvider localContentProvider,
        ITimelessContentProvider timelessContentProvider)
    {
        InitRootDrives();

        var localContentProviderAsList = new SourceCache<AbsolutePath, string>(i => i.Path.Path);
        localContentProviderAsList.AddOrUpdate(new AbsolutePath(timelessContentProvider, localContentProvider));
        var localContentProviderStream = localContentProviderAsList.Connect();

        var rootDriveInfos = Observable.CombineLatest(
                localContentProvider.Items,
                _rootDrives.Connect().StartWithEmpty().ToCollection(),
                (items, drives) =>
                {
                    return items is null
                        ? Observable.Empty<IChangeSet<(AbsolutePath Path, DriveInfo? Drive), string>>()
                        : items!
                            .Or(new[] { localContentProviderStream })
                            .Transform(i => (Path: i, Drive: drives.FirstOrDefault(d =>
                            {
                                var containerPath = localContentProvider.GetNativePath(i.Path).Path;
                                var drivePath = d.Name.TrimEnd(Path.DirectorySeparatorChar);
                                return containerPath == drivePath
                                       || (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && containerPath == "/" &&
                                           d.Name == "/");
                            })))
                            .Filter(t => t.Drive is not null);
                }
            )
            .Switch()
            .TransformAsync(async t => (Item: await t.Path.ResolveAsyncSafe(), Drive: t.Drive!))
            .Filter(t => t.Item is IContainer)
            .Transform(t => (Container: (IContainer)t.Item!, t.Drive))
            .Transform(t => new RootDriveInfo(t.Drive, t.Container))
            .Sort(SortExpressionComparer<RootDriveInfo>.Ascending(d => d.Name));

        guiAppState.RootDriveInfos = new BindedCollection<RootDriveInfo, string>(rootDriveInfos);

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

            _rootDrives.AddRange(drives);
        }
    }

    public Task InitAsync() => Task.CompletedTask;
}