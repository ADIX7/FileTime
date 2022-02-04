using AsyncEvent;
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
        private readonly SmbShare _smbShare;
        private readonly IContainer? _parent;

        public string Name { get; }

        public string? FullName { get; }

        public bool IsHidden => false;
        public bool IsLoaded => _items != null;

        public SmbContentProvider Provider { get; }
        IContentProvider IItem.Provider => Provider;
        public SupportsDelete CanDelete => SupportsDelete.True;
        public bool CanRename => true;

        public AsyncEventHandler Refreshed { get; } = new();
        public IReadOnlyList<Exception> Exceptions { get; } = new List<Exception>().AsReadOnly();

        public bool IsDisposed { get; private set; }

        public bool SupportsDirectoryLevelSoftDelete => false;

        public SmbFolder(string name, SmbContentProvider contentProvider, SmbShare smbShare, IContainer parent)
        {
            _parent = parent;
            _smbShare = smbShare;

            Name = name;
            FullName = parent?.FullName == null ? Name : parent.FullName + Constants.SeparatorChar + Name;
            Provider = contentProvider;
        }

        public Task<IContainer> CreateContainer(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IElement> CreateElement(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IContainer> Clone() => Task.FromResult((IContainer)this);

        public async Task<IItem?> GetByPath(string path, bool acceptDeepestMatch = false)
        {
            var paths = path.Split(Constants.SeparatorChar);

            var item = (await GetItems())?.FirstOrDefault(i => i.Name == paths[0]);

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

        public IContainer? GetParent() => _parent;

        public Task<bool> IsExists(string name)
        {
            throw new NotImplementedException();
        }

        public Task Delete(bool hardDelete = false)
        {
            throw new NotImplementedException();
        }
        public Task Rename(string newName)
        {
            throw new NotImplementedException();
        }

        public async Task RefreshAsync(CancellationToken token = default)
        {
            var containers = new List<IContainer>();
            var elements = new List<IElement>();

            try
            {
                var path = FullName![(_smbShare.FullName!.Length + 1)..];
                (containers, elements) = await _smbShare.ListFolder(this, _smbShare.Name, path, token);
            }
            catch { }

            _containers = containers.AsReadOnly();
            _elements = elements.AsReadOnly();

            if (_items != null)
            {
                foreach (var item in _items)
                {
                    item.Dispose();
                }
            }

            _items = _containers.Cast<IItem>().Concat(_elements).ToList().AsReadOnly();
            await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty, token);
        }

        public async Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default)
        {
            if (_items == null) await RefreshAsync();
            return _items;
        }
        public async Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default)
        {
            if (_containers == null) await RefreshAsync();
            return _containers;
        }
        public async Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default)
        {
            if (_elements == null) await RefreshAsync();
            return _elements;
        }
        public Task<bool> CanOpen() => Task.FromResult(true);

        public void Dispose() => IsDisposed = true;
    }
}