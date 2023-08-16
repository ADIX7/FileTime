namespace FileTime.Core.Models;

public readonly struct VolumeSizeInfo
{
    public readonly long TotalSize;
    public readonly long FreeSize;
    
    public VolumeSizeInfo(long totalSize, long freeSize)
    {
        TotalSize = totalSize;
        FreeSize = freeSize;
    }
}