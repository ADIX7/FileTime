using FileTime.App.Core.Models;
using FileTime.Core.Models;
using FileTime.Providers.Local;
using MvvmGen;
using System.IO;
using System.Runtime.InteropServices;

namespace FileTime.Avalonia.Models
{
    [ViewModel]
    public partial class RootDriveInfo : IHaveContainer
    {
        private readonly DriveInfo _driveInfo;
        private readonly IContainer _container;

        public IContainer Container => _container;

        [Property]
        private string _name;

        [Property]
        private string _fullName;

        [Property]
        private string _label;

        [Property]
        private long _size;

        [Property]
        private long _free;

        [Property]
        private long _used;

        [PropertyInvalidate(nameof(Used))]
        [PropertyInvalidate(nameof(Size))]
        public long UsedPercentage => Size == 0 ? 0 : Used * 100 / Size;

        public RootDriveInfo(DriveInfo driveInfo, IContainer container)
        {
            _driveInfo = driveInfo;
            _container = container;

            Refresh();
        }

        private void Refresh()
        {
            Name = _container.Name;
            FullName = _container is LocalContentProvider ? "/" : _container.FullName;
            Label = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? _driveInfo.VolumeLabel : null;
            Size = _driveInfo.TotalSize;
            Free = _driveInfo.AvailableFreeSpace;
            Used = _driveInfo.TotalSize - _driveInfo.AvailableFreeSpace;
        }
    }
}
