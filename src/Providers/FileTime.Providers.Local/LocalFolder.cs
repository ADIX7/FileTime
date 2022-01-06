using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Providers.Local
{
    public class LocalFolder : IContainer
    {
        private IReadOnlyList<IItem>? _items;
        private IReadOnlyList<IContainer>? _containers;
        private IReadOnlyList<IElement>? _elements;
        private readonly IContainer? _parent;

        public bool IsHidden => (Directory.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
        public DirectoryInfo Directory { get; }
        public IContentProvider Provider { get; }

        public IReadOnlyList<IItem> Items
        {
            get
            {
                if (_items == null) Refresh();
                return _items!;
            }

            private set => _items = value;
        }

        public IReadOnlyList<IContainer> Containers
        {
            get
            {
                if (_containers == null) Refresh();
                return _containers!;
            }

            private set => _containers = value;
        }

        public IReadOnlyList<IElement> Elements
        {
            get
            {
                if (_elements == null) Refresh();
                return _elements!;
            }

            private set => _elements = value;
        }

        public string Name { get; }

        public string FullName { get; }

        public event EventHandler? Refreshed;

        public LocalFolder(DirectoryInfo directory, IContentProvider contentProvider, IContainer? parent)
        {
            Directory = directory;
            _parent = parent;

            Name = directory.Name;
            FullName = parent?.FullName == null ? Name : parent.FullName + Constants.SeparatorChar + Name;
            Provider = contentProvider;
        }

        public IContainer? GetParent() => _parent;

        public void Refresh()
        {
            _containers = new List<IContainer>();
            _elements = new List<IElement>();

            try
            {
                _containers = Directory.GetDirectories().Select(d => new LocalFolder(d, Provider, this)).OrderBy(d => d.Name).ToList().AsReadOnly();
                _elements = Directory.GetFiles().Select(f => new LocalFile(f, Provider)).OrderBy(f => f.Name).ToList().AsReadOnly();
            }
            catch { }

            _items = _containers.Cast<IItem>().Concat(_elements).ToList().AsReadOnly();
            Refreshed?.Invoke(this, EventArgs.Empty);
        }

        public IItem? GetByPath(string path)
        {
            var paths = path.Split(Constants.SeparatorChar);

            var item = Items.FirstOrDefault(i => i.Name == paths[0]);

            if (paths.Length == 1)
            {
                return item;
            }

            if (item is IContainer container)
            {
                return container.GetByPath(string.Join(Constants.SeparatorChar, paths.Skip(1)));
            }

            return null;
        }
        public IContainer CreateContainer(string name)
        {
            Directory.CreateSubdirectory(name);
            Refresh();

            return _containers!.FirstOrDefault(c => c.Name == name)!;
        }

        public IElement CreateElement(string name)
        {
            using (File.Create(Path.Combine(Directory.FullName, name))) { }
            Refresh();

            return _elements!.FirstOrDefault(e => e.Name == name)!;
        }

        public bool IsExists(string name) => Items.Any(i => i.Name == name);

        public void Delete()
        {
            Directory.Delete(true);
        }
    }
}