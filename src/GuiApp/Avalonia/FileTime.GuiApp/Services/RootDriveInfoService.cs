using System.Reactive.Linq;
using System.Runtime.InteropServices;
using DynamicData;
using FileTime.App.Core.Models;
using FileTime.App.Core.Services;
using FileTime.Core.Models;
using FileTime.GuiApp.ViewModels;
using FileTime.Providers.Local;
using IContainer = FileTime.Core.Models.IContainer;

namespace FileTime.GuiApp.Services;

public class RootDriveInfoService : IStartupHandler
{
    private readonly SourceList<DriveInfo> _rootDrives = new();

    public RootDriveInfoService(IGuiAppState guiAppState, ILocalContentProvider localContentProvider)
    {
        InitRootDrives();

        var rootDriveInfos = Observable.CombineLatest(
            localContentProvider.Items,
            _rootDrives.Connect().StartWithEmpty().ToCollection(),
            (items, drives) =>
            {
                return items is null
                    ? Observable.Empty<IChangeSet<(IAbsolutePath Path, DriveInfo? Drive)>>()
                    : items
                        .Transform(i => (Path: i, Drive: drives.FirstOrDefault(d =>
                        {
                            var asd1 = localContentProvider.GetNativePath(i.Path).Path;
                            var asd2 = d.Name.TrimEnd(Path.DirectorySeparatorChar);
                            return asd1 == asd2;
                        })))
                        .Filter(t => t.Drive is not null);
            }
        )
        .Switch()
        .TransformAsync(async t => (Item: await t.Path.ResolveAsyncSafe(), Drive: t.Drive!))
        .Filter(t => t.Item is IContainer)
        .Transform(t => (Container: (IContainer)t.Item!, t.Drive))
        .Transform(t => new RootDriveInfo(t.Drive, t.Container));

        guiAppState.RootDriveInfos = new BindedCollection<RootDriveInfo>(rootDriveInfos);

        void InitRootDrives()
        {
            var driveInfos = new List<RootDriveInfo>();
            IEnumerable<DriveInfo> drives = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
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
}