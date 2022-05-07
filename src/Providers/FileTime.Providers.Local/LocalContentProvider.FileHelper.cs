using System.Runtime.InteropServices;

namespace FileTime.Providers.Local;

partial class LocalContentProvider
{
    private static string GetFileAttributes(FileInfo fileInfo)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "";
        }
        else
        {
            return "-"
                   + ((fileInfo.Attributes & FileAttributes.Archive) == FileAttributes.Archive ? "a" : "-")
                   + ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ? "r" : "-")
                   + ((fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden ? "h" : "-")
                   + ((fileInfo.Attributes & FileAttributes.System) == FileAttributes.System ? "s" : "-");
        }
    }
}