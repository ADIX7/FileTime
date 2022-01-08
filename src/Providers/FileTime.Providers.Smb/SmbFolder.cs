using FileTime.Core.Models;
using FileTime.Core.Providers;
using SMBLibrary;
using SMBLibrary.Client;

namespace FileTime.Providers.Smb
{
    public class SmbFolder : IContainer
    {
        private IReadOnlyList<IItem>? _items;
        private IReadOnlyList<IContainer>? _containers;
        private IReadOnlyList<IElement>? _elements;
        private Func<ISMBClient> _getSmbClient;
        private readonly SmbShare _smbShare;
        private readonly IContainer? _parent;

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

        public string? FullName { get; }

        public bool IsHidden => false;

        public SmbContentProvider Provider { get; }
        IContentProvider IItem.Provider => Provider;

        public event EventHandler? Refreshed;

        public SmbFolder(string name, SmbContentProvider contentProvider, SmbShare smbShare, IContainer parent, Func<ISMBClient> getSmbClient)
        {
            _parent = parent;
            _getSmbClient = getSmbClient;

            Name = name;
            FullName = parent?.FullName == null ? Name : parent.FullName + Constants.SeparatorChar + Name;
            Provider = contentProvider;
            _smbShare = smbShare;
        }

        public IContainer CreateContainer(string name)
        {
            throw new NotImplementedException();
        }

        public IElement CreateElement(string name)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
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

        public IContainer? GetParent() => _parent;

        public bool IsExists(string name)
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            var containers = new List<IContainer>();
            var elements = new List<IElement>();

            try
            {
                var path = FullName![(_smbShare.FullName!.Length + 1)..];
                (containers, elements) = _smbShare.ListFolder(this, _smbShare.Name, path);
            }
            catch { }

            _containers = containers.AsReadOnly();
            _elements = elements.AsReadOnly();

            _items = _containers.Cast<IItem>().Concat(_elements).ToList().AsReadOnly();
            Refreshed?.Invoke(this, EventArgs.Empty);
        }
    }
}