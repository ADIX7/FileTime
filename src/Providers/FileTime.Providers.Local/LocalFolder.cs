using System.Runtime.InteropServices;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using FileTime.Core.Providers.ContainerProperty;
using FileTime.Providers.Local.Interop;

namespace FileTime.Providers.Local
{
    public class LocalFolder : AbstractContainer<LocalContentProvider>, IContainer, IHaveCreatedAt, IHaveAttributes
    {
        public DirectoryInfo Directory { get; }

        public string Attributes => GetAttributes();

        public DateTime? CreatedAt => Directory.CreationTime;
        public override bool IsExists => Directory.Exists;

        public LocalFolder(DirectoryInfo directory, LocalContentProvider contentProvider, IContainer parent)
         : base(contentProvider, parent, directory.Name.TrimEnd(Path.DirectorySeparatorChar))
        {
            Directory = directory;
            NativePath = Directory.FullName;
            IsHidden = (Directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            CanRename = true;

            //TODO: Linux soft delete
            SupportsDirectoryLevelSoftDelete = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            CanDelete = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? SupportsDelete.True
                : SupportsDelete.HardDeleteOnly;
        }

        public override Task<IContainer> CloneAsync() => Task.FromResult((IContainer)new LocalFolder(Directory, Provider, GetParent()!));

        public override Task<IEnumerable<IItem>> RefreshItems(CancellationToken token = default)
        {
            try
            {
                if (token.IsCancellationRequested) return Task.FromResult(Enumerable.Empty<IItem>());
                var containers = Directory.GetDirectories().Select(d => new LocalFolder(d, Provider, this)).OrderBy(d => d.Name).ToList().AsReadOnly();

                if (token.IsCancellationRequested) return Task.FromResult(Enumerable.Empty<IItem>());
                var elements = Directory.GetFiles().Select(f => new LocalFile(f, this, Provider)).OrderBy(f => f.Name).ToList().AsReadOnly();

                if (token.IsCancellationRequested) return Task.FromResult(Enumerable.Empty<IItem>());

                return Task.FromResult(containers.Cast<IItem>().Concat(elements));
            }
            catch (Exception e)
            {
                AddException(e);
            }

            return Task.FromResult(Enumerable.Empty<IItem>());
        }

        async Task<IItem?> IContainer.GetByPath(string path, bool acceptDeepestMatch)
        {
            var paths = path.Split(Constants.SeparatorChar);

            var item = (await GetItems())!.FirstOrDefault(i => Provider.NormalizePath(i.Name) == Provider.NormalizePath(paths[0]));

            if (paths.Length == 1)
            {
                return item;
            }

            if (item is IContainer container)
            {
                var result = await container.GetByPath(string.Join(Constants.SeparatorChar, paths.Skip(1)), acceptDeepestMatch);
                return result == null && acceptDeepestMatch ? this : result;
            }

            return null;
        }
        public override async Task<IContainer> CreateContainerAsync(string name)
        {
            Directory.CreateSubdirectory(name);
            await RefreshAsync();

            return (await GetContainers())!.FirstOrDefault(c => Provider.NormalizePath(c.Name) == Provider.NormalizePath(name))!;
        }

        public override async Task<IElement> CreateElementAsync(string name)
        {
            using (File.Create(Path.Combine(Directory.FullName, name))) { }
            await RefreshAsync();

            return (await GetElements())!.FirstOrDefault(e => Provider.NormalizePath(e.Name) == Provider.NormalizePath(name))!;
        }

        public override async Task<bool> IsExistsAsync(string name) => (await GetItems())?.Any(i => i.IsExists && Provider.NormalizePath(i.Name) == Provider.NormalizePath(name)) ?? false;

        public override Task Delete(bool hardDelete = false)
        {
            if (hardDelete)
            {
                Directory.Delete(true);
            }
            else
            {
                WindowsInterop.MoveToRecycleBin(Directory.FullName);
            }
            return Task.CompletedTask;
        }
        public override async Task Rename(string newName)
        {
            if (GetParent() is LocalFolder parentFolder)
            {
                System.IO.Directory.Move(Directory.FullName, Path.Combine(parentFolder.Directory.FullName, newName));
                await parentFolder.RefreshAsync();
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
                return "d"
                    + ((Directory.Attributes & FileAttributes.Archive) == FileAttributes.Archive ? "a" : "-")
                    + ((Directory.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ? "r" : "-")
                    + ((Directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden ? "h" : "-")
                    + ((Directory.Attributes & FileAttributes.System) == FileAttributes.System ? "s" : "-");
            }
        }
    }
}