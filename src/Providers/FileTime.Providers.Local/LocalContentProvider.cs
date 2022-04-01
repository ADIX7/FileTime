using System.Runtime.InteropServices;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Services;

namespace FileTime.Providers.Local
{
    public partial class LocalContentProvider : ContentProviderBase, ILocalContentProvider
    {
        protected bool IsCaseInsensitive { get; init; }
        public LocalContentProvider() : base("local")
        {
            IsCaseInsensitive = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            RefreshRootDirectories();
        }

        public override Task OnEnter()
        {
            RefreshRootDirectories();

            return Task.CompletedTask;
        }

        private void RefreshRootDirectories()
        {
            var rootDirectories = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? new DirectoryInfo("/").GetDirectories()
                : Environment.GetLogicalDrives().Select(d => new DirectoryInfo(d));

            Items.Clear();
            Items.AddRange(rootDirectories.Select(DirectoryToAbsolutePath));
        }

        public override Task<IItem> GetItemByNativePathAsync(NativePath nativePath)
        {
            var path = nativePath.Path;
            if (Directory.Exists(path))
            {
                return Task.FromResult((IItem)DirectoryToContainer(new DirectoryInfo(path)));
            }
            else if (File.Exists(path))
            {
                return Task.FromResult((IItem)FileToElement(new FileInfo(path)));
            }

            throw new FileNotFoundException("Directory or file not found", path);
        }

        public override Task<List<IAbsolutePath>> GetItemsByContainerAsync(FullName fullName) => Task.FromResult(GetItemsByContainer(fullName));
        public List<IAbsolutePath> GetItemsByContainer(FullName fullName) => GetItemsByContainer(new DirectoryInfo(GetNativePath(fullName).Path));
        public List<IAbsolutePath> GetItemsByContainer(DirectoryInfo directoryInfo) => directoryInfo.GetDirectories().Select(DirectoryToAbsolutePath).Concat(directoryInfo.GetFiles().Select(FileToAbsolutePath)).ToList();

        private IAbsolutePath DirectoryToAbsolutePath(DirectoryInfo directoryInfo)
        {
            var fullName = GetFullName(directoryInfo);
            return new AbsolutePath(this, fullName, AbsolutePathType.Container);
        }

        private IAbsolutePath FileToAbsolutePath(FileInfo file)
        {
            var fullName = GetFullName(file);
            return new AbsolutePath(this, fullName, AbsolutePathType.Element);
        }

        private Container DirectoryToContainer(DirectoryInfo directoryInfo)
        {
            var fullName = GetFullName(directoryInfo.FullName);
            return new(
                directoryInfo.Name,
                directoryInfo.Name,
                fullName,
                new(directoryInfo.FullName),
                fullName.GetParent()!,
                (directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
                directoryInfo.Exists,
                directoryInfo.CreationTime,
                SupportsDelete.True,
                true,
                GetDirectoryAttributes(directoryInfo),
                this,
                GetItemsByContainer(directoryInfo)
            );
        }

        private Element FileToElement(FileInfo fileInfo)
        {
            var fullName = GetFullName(fileInfo);
            return new(
                fileInfo.Name,
                fileInfo.Name,
                fullName,
                new(fileInfo.FullName),
                fullName.GetParent()!,
                (fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
                fileInfo.Exists,
                fileInfo.CreationTime,
                SupportsDelete.True,
                true,
                GetFileAttributes(fileInfo),
                this
            );
        }

        private FullName GetFullName(DirectoryInfo directoryInfo) => GetFullName(directoryInfo.FullName);
        private FullName GetFullName(FileInfo fileInfo) => GetFullName(fileInfo.FullName);
        private FullName GetFullName(NativePath nativePath) => GetFullName(nativePath.Path);
        private FullName GetFullName(string nativePath) => new(Name + Constants.SeparatorChar + string.Join(Constants.SeparatorChar, nativePath.Split(Path.DirectorySeparatorChar)));

        public override NativePath GetNativePath(FullName fullName) => new(string.Join(Path.DirectorySeparatorChar, fullName.Path.Split(Constants.SeparatorChar).Skip(1)));
    }
}