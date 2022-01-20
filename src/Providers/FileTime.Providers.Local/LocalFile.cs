using System.Runtime.InteropServices;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using FileTime.Providers.Local.Extensions;
using Mono.Unix;

namespace FileTime.Providers.Local
{
    public class LocalFile : IElement
    {
        private readonly FileInfo _file;

        public FileInfo File => _file;

        public string Name { get; }

        public string FullName { get; }

        public IContentProvider Provider { get; }

        public bool IsHidden => (_file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
        public bool IsSpecial =>
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                && (new UnixFileInfo(_file.FullName).FileAccessPermissions & FileAccessPermissions.UserExecute) == FileAccessPermissions.UserExecute;

        public LocalFile(FileInfo file, IContentProvider contentProvider)
        {
            _file = file;

            Name = file.Name;
            FullName = file.FullName;
            Provider = contentProvider;
        }

        public string GetPrimaryAttributeText() => _file.Length.ToSizeString();

        public void Delete()
        {
            _file.Delete();
        }
    }
}