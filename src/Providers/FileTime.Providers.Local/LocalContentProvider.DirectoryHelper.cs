using System.Runtime.InteropServices;

namespace FileTime.Providers.Local
{
    partial class LocalContentProvider
    {
        private static string GetDirectoryAttributes(DirectoryInfo directoryInfo)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "";
            }
            else
            {
                return "d"
                    + ((directoryInfo.Attributes & FileAttributes.Archive) == FileAttributes.Archive ? "a" : "-")
                    + ((directoryInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ? "r" : "-")
                    + ((directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden ? "h" : "-")
                    + ((directoryInfo.Attributes & FileAttributes.System) == FileAttributes.System ? "s" : "-");
            }
        }

        private static IEnumerable<FileInfo> GetFilesSafe(DirectoryInfo directoryInfo)
        {
            try
            {
                return directoryInfo.GetFiles();
            }
            catch
            {
                return Enumerable.Empty<FileInfo>();
            }
        }
    }
}