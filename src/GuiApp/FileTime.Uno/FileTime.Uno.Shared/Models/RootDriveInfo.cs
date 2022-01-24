using FileTime.Core.Models;
using MvvmGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileTime.Uno.Models
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

            Name = container.Name;
            FullName = container.FullName;
            Size = driveInfo.TotalSize;
            Free = driveInfo.AvailableFreeSpace;
            Used = driveInfo.TotalSize - driveInfo.AvailableFreeSpace;
        }
    }
}
