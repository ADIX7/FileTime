using System.Reactive.Linq;
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

            Items.OnNext(rootDirectories.Select(DirectoryToAbsolutePath).ToList());
        }

        public override Task<IItem> GetItemByNativePathAsync(NativePath nativePath, bool forceResolve = false, AbsolutePathType forceResolvePathType = AbsolutePathType.Unknown)
        {
            var path = nativePath.Path;
            try
            {
                if ((path?.Length ?? 0) == 0)
                {
                    return Task.FromResult((IItem)this);
                }
                else if (Directory.Exists(path))
                {
                    return Task.FromResult((IItem)DirectoryToContainer(new DirectoryInfo(path!.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar)));
                }
                else if (File.Exists(path))
                {
                    return Task.FromResult((IItem)FileToElement(new FileInfo(path)));
                }

                var type = forceResolvePathType switch
                {
                    AbsolutePathType.Container => "Directory",
                    AbsolutePathType.Element => "File",
                    _ => "Directory or file"
                };

                if (forceResolvePathType == AbsolutePathType.Container) throw new DirectoryNotFoundException($"{type} not found: '{path}'");
                throw new FileNotFoundException(type + " not found", path);
            }
            catch (Exception e)
            {
                if (!forceResolve) throw new Exception($"Could not resolve path '{nativePath.Path}' and {nameof(forceResolve)} is false.", e);

                return forceResolvePathType switch
                {
                    AbsolutePathType.Container => Task.FromResult((IItem)CreateEmptyContainer(nativePath, Observable.Return(new List<Exception>() { e }))),
                    AbsolutePathType.Element => Task.FromResult(CreateEmptyElement(nativePath)),
                    _ => throw new Exception($"Could not resolve path '{nativePath.Path}' and could not force create, because {nameof(forceResolvePathType)} is {nameof(AbsolutePathType.Unknown)}.", e)
                };
            }
        }

        private Container CreateEmptyContainer(NativePath nativePath, IObservable<IEnumerable<Exception>>? exceptions = null)
        {
            var nonNullExceptions = exceptions ?? Observable.Return(Enumerable.Empty<Exception>());
            var name = nativePath.Path.Split(Path.DirectorySeparatorChar).LastOrDefault() ?? "???";
            var fullName = GetFullName(nativePath);

            var parentFullName = fullName.GetParent();
            var parent = new AbsolutePath(
                this,
                parentFullName ?? new FullName(""),
                AbsolutePathType.Container);

            return new Container(
                name,
                name,
                fullName,
                nativePath,
                parent,
                false,
                true,
                DateTime.MinValue,
                SupportsDelete.False,
                false,
                "???",
                this,
                nonNullExceptions,
                Observable.Return<IEnumerable<IAbsolutePath>?>(null)
            );
        }

        private IItem CreateEmptyElement(NativePath nativePath)
        {
            throw new NotImplementedException();
        }

        public override Task<List<IAbsolutePath>> GetItemsByContainerAsync(FullName fullName) => Task.FromResult(GetItemsByContainer(fullName));
        public List<IAbsolutePath> GetItemsByContainer(FullName fullName) => GetItemsByContainer(new DirectoryInfo(GetNativePath(fullName).Path));
        public List<IAbsolutePath> GetItemsByContainer(DirectoryInfo directoryInfo) => directoryInfo.GetDirectories().Select(DirectoryToAbsolutePath).Concat(GetFilesSafe(directoryInfo).Select(FileToAbsolutePath)).ToList();

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
            var parentFullName = fullName.GetParent();
            var parent = new AbsolutePath(
                this,
                parentFullName ?? new FullName(""),
                AbsolutePathType.Container);

            return new(
                directoryInfo.Name,
                directoryInfo.Name,
                fullName,
                new(directoryInfo.FullName),
                parent,
                (directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
                directoryInfo.Exists,
                directoryInfo.CreationTime,
                SupportsDelete.True,
                true,
                GetDirectoryAttributes(directoryInfo),
                this,
                Observable.Return(Enumerable.Empty<Exception>()),
                Observable.Return(GetItemsByContainer(directoryInfo))
            );
        }

        private Element FileToElement(FileInfo fileInfo)
        {
            var fullName = GetFullName(fileInfo);
            var parentFullName = fullName.GetParent() ?? throw new Exception($"Path does not have parent: '{fileInfo.FullName}'");
            var parent = new AbsolutePath(this, parentFullName, AbsolutePathType.Container);

            return new(
                fileInfo.Name,
                fileInfo.Name,
                fullName,
                new(fileInfo.FullName),
                parent,
                (fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
                fileInfo.Exists,
                fileInfo.CreationTime,
                SupportsDelete.True,
                true,
                GetFileAttributes(fileInfo),
                this,
                Observable.Return(Enumerable.Empty<Exception>())
            );
        }

        private FullName GetFullName(DirectoryInfo directoryInfo) => GetFullName(directoryInfo.FullName);
        private FullName GetFullName(FileInfo fileInfo) => GetFullName(fileInfo.FullName);
        private FullName GetFullName(NativePath nativePath) => GetFullName(nativePath.Path);
        private FullName GetFullName(string nativePath) => new((Name + Constants.SeparatorChar + string.Join(Constants.SeparatorChar, nativePath.Split(Path.DirectorySeparatorChar))).TrimEnd(Constants.SeparatorChar));

        public override NativePath GetNativePath(FullName fullName) => new(string.Join(Path.DirectorySeparatorChar, fullName.Path.Split(Constants.SeparatorChar).Skip(1)));
    }
}