using System.Runtime.InteropServices;
using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Providers.Local
{
    public class LocalFolder : IContainer
    {
        private IReadOnlyList<IItem>? _items;
        private IReadOnlyList<IContainer>? _containers;
        private IReadOnlyList<IElement>? _elements;
        private List<Exception> _exceptions;
        private readonly IContainer? _parent;

        public bool IsHidden => (Directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
        public DirectoryInfo Directory { get; }
        public LocalContentProvider Provider { get; }
        IContentProvider IItem.Provider => Provider;

        public string Name { get; }

        public string FullName { get; }

        public bool IsLoaded => _items != null;
        public bool CanDelete => true;
        public bool CanRename => true;

        public AsyncEventHandler Refreshed { get; } = new();

        public string Attributes => GetAttributes();

        public DateTime CreatedAt => Directory.CreationTime;
        public IReadOnlyList<Exception> Exceptions { get; }

        public LocalFolder(DirectoryInfo directory, LocalContentProvider contentProvider, IContainer? parent)
        {
            Directory = directory;
            _parent = parent;

            _exceptions = new List<Exception>();
            Exceptions = _exceptions.AsReadOnly();

            Name = directory.Name.TrimEnd(Path.DirectorySeparatorChar);
            FullName = parent?.FullName == null ? Name : parent.FullName + Constants.SeparatorChar + Name;
            Provider = contentProvider;
        }

        public IContainer? GetParent() => _parent;

        public Task<IContainer> Clone() => Task.FromResult((IContainer)new LocalFolder(Directory, Provider, _parent));

        public Task Refresh()
        {
            _containers = new List<IContainer>();
            _elements = new List<IElement>();
            _exceptions.Clear();

            try
            {
                _containers = Directory.GetDirectories().Select(d => new LocalFolder(d, Provider, this)).OrderBy(d => d.Name).ToList().AsReadOnly();
                _elements = Directory.GetFiles().Select(f => new LocalFile(f, this, Provider)).OrderBy(f => f.Name).ToList().AsReadOnly();
            }
            catch (Exception e)
            {
                _exceptions.Add(e);
            }

            _items = _containers.Cast<IItem>().Concat(_elements).ToList().AsReadOnly();
            Refreshed?.InvokeAsync(this, AsyncEventArgs.Empty);

            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default)
        {
            if (_items == null) await Refresh();
            return _items;
        }
        public async Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default)
        {
            if (_containers == null) await Refresh();
            return _containers;
        }
        public async Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default)
        {
            if (_elements == null) await Refresh();
            return _elements;
        }

        public async Task<IItem?> GetByPath(string path, bool acceptDeepestMatch = false)
        {
            var paths = path.Split(Constants.SeparatorChar);

            var item = (await GetItems())!.FirstOrDefault(i => Provider.NormalizePath(i.Name) == Provider.NormalizePath(paths[0]));

            if (paths.Length == 1)
            {
                return item;
            }

            if (item is IContainer container)
            {
                return await container.GetByPath(string.Join(Constants.SeparatorChar, paths.Skip(1)), acceptDeepestMatch);
            }

            return null;
        }
        public async Task<IContainer> CreateContainer(string name)
        {
            Directory.CreateSubdirectory(name);
            await Refresh();

            return _containers!.FirstOrDefault(c => Provider.NormalizePath(c.Name) == Provider.NormalizePath(name))!;
        }

        public async Task<IElement> CreateElement(string name)
        {
            using (File.Create(Path.Combine(Directory.FullName, name))) { }
            await Refresh();

            return _elements!.FirstOrDefault(e => Provider.NormalizePath(e.Name) == Provider.NormalizePath(name))!;
        }

        public async Task<bool> IsExists(string name) => (await GetItems())?.Any(i => Provider.NormalizePath(i.Name) == Provider.NormalizePath(name)) ?? false;

        public Task Delete()
        {
            Directory.Delete(true);
            return Task.CompletedTask;
        }
        public async Task Rename(string newName)
        {
            if (_parent is LocalFolder parentFolder)
            {
                System.IO.Directory.Move(Directory.FullName, Path.Combine(parentFolder.Directory.FullName, newName));
                await _parent.Refresh();
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
        public Task<bool> CanOpen() => Task.FromResult(true);
    }
}