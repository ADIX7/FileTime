using FileTime.Core.Models;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileTime.Avalonia.Models
{
    [ViewModel]
    public partial class RootDriveInfo
    {
        private readonly DriveInfo _driveInfo;
        private readonly IContainer _container;

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
        public long UsedPercentage => Used * 100 / Size;

        public RootDriveInfo(DriveInfo driveInfo, IContainer container)
        {
            _driveInfo = driveInfo;
            _container = container;

            Refresh();
        }

        private void Refresh()
        {
            Name = _container.Name;
            FullName = _container.FullName;
            Label = _driveInfo.VolumeLabel;
            Size = _driveInfo.TotalSize;
            Free = _driveInfo.AvailableFreeSpace;
            Used = _driveInfo.TotalSize - _driveInfo.AvailableFreeSpace;
        }
    }
}
