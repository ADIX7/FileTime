using System.Runtime.InteropServices;
using FileTime.Core.Models;
using PropertyChanged.SourceGenerator;
using IContainer = FileTime.Core.Models.IContainer;

namespace FileTime.Providers.Local;

public partial class RootDriveInfo
{
    private readonly DriveInfo _driveInfo;

    [Notify] private string _name;

    [Notify] private string _fullName;

    [Notify] private string? _label;

    [Notify] private long _size;

    [Notify] private long _free;

    [Notify] private long _used;
    
    public DriveType DriveType => _driveInfo.DriveType;

    public long UsedPercentage => Size == 0 ? 0 : Used * 100 / Size;

    public FullName Path { get; }

    public RootDriveInfo(DriveInfo driveInfo, IContainer container)
    {
        _driveInfo = driveInfo;

        _name = container.Name;

        _fullName = driveInfo.Name;

        Path = container.FullName ?? throw new NullReferenceException($"Container does not have a {nameof(FullName)}");

        Refresh();
    }

    private void Refresh()
    {
        Label = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? _driveInfo.VolumeLabel : null;
        Size = _driveInfo.TotalSize;
        Free = _driveInfo.AvailableFreeSpace;
        Used = _driveInfo.TotalSize - _driveInfo.AvailableFreeSpace;
    }
}