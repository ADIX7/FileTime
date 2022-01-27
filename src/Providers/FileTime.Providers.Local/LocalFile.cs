using System.Runtime.InteropServices;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using FileTime.Providers.Local.Extensions;
using Mono.Unix;

namespace FileTime.Providers.Local
{
    public class LocalFile : IElement
    {
        public FileInfo File { get; }

        public string Name { get; }

        public string FullName { get; }

        public IContentProvider Provider { get; }

        public bool IsHidden => (File.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
        public bool IsSpecial =>
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                && (new UnixFileInfo(File.FullName).FileAccessPermissions & FileAccessPermissions.UserExecute) == FileAccessPermissions.UserExecute;

        public string Attributes => GetAttributes();

        public DateTime CreatedAt => File.CreationTime;

        public LocalFile(FileInfo file, IContentProvider contentProvider)
        {
            File = file;

            Name = file.Name;
            FullName = file.FullName;
            Provider = contentProvider;
        }

        public string GetPrimaryAttributeText() => File.Length.ToSizeString();

        public Task Delete()
        {
            File.Delete();
            return Task.CompletedTask;
        }

        public string GetAttributes()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "";
            }
            else
            {
                return "-"
                    + ((File.Attributes & FileAttributes.Archive) == FileAttributes.Archive ? "a" : "-")
                    + ((File.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ? "r" : "-")
                    + ((File.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden ? "h" : "-")
                    + ((File.Attributes & FileAttributes.System) == FileAttributes.System ? "s" : "-");
            }
        }
    }
}