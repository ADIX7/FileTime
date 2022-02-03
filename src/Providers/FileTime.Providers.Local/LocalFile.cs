using System.Runtime.InteropServices;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using FileTime.Providers.Local.Extensions;
using FileTime.Providers.Local.Interop;
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
        public SupportsDelete CanDelete => SupportsDelete.True;
        public bool CanRename => true;

        private readonly LocalFolder _parent;

        public LocalFile(FileInfo file, LocalFolder parent, IContentProvider contentProvider)
        {
            _parent = parent;
            File = file;

            Name = file.Name;
            FullName = parent.FullName + Constants.SeparatorChar + file.Name;
            Provider = contentProvider;
        }

        public string GetPrimaryAttributeText() => File.Length.ToSizeString();

        public Task Delete(bool hardDelete = false)
        {
            if (hardDelete)
            {
                File.Delete();
            }
            else
            {
                WindowsInterop.MoveToRecycleBin(File.FullName);
            }
            return Task.CompletedTask;
        }
        public async Task Rename(string newName)
        {
            if (_parent is LocalFolder parentFolder)
            {
                System.IO.File.Move(File.FullName, Path.Combine(parentFolder.Directory.FullName, newName));
                await _parent.RefreshAsync();
            }
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

        public IContainer? GetParent() => _parent;
    }
}