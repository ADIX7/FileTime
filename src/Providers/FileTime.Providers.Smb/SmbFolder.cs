using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Providers.Smb
{
    public class SmbFolder : IContainer
    {
        private IReadOnlyList<IItem>? _items;
        private IReadOnlyList<IContainer>? _containers;
        private IReadOnlyList<IElement>? _elements;
        private readonly IContainer? _parent;

        public string Name { get; }

        public string? FullName { get; }
        public string? NativePath { get; }

        public bool IsHidden => false;
        public bool IsLoaded => _items != null;

        public SmbContentProvider Provider { get; }
        IContentProvider IItem.Provider => Provider;
        public SmbShare SmbShare { get; }
        public SupportsDelete CanDelete => SupportsDelete.True;
        public bool CanRename => true;

        public AsyncEventHandler Refreshed { get; } = new();
        public IReadOnlyList<Exception> Exceptions { get; } = new List<Exception>().AsReadOnly();

        public bool IsDestroyed { get; private set; }

        public bool SupportsDirectoryLevelSoftDelete => false;

        public SmbFolder(string name, SmbContentProvider contentProvider, SmbShare smbShare, IContainer parent)
        {
            _parent = parent;
            SmbShare = smbShare;

            Name = name;
            FullName = parent?.FullName == null ? Name : parent.FullName + Constants.SeparatorChar + Name;
            NativePath = SmbContentProvider.GetNativePath(FullName);
            Provider = contentProvider;
        }

        public Task<IContainer> CreateContainerAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IElement> CreateElementAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IContainer> CloneAsync() => Task.FromResult((IContainer)this);

        public IContainer? GetParent() => _parent;

        public Task<bool> IsExistsAsync(string name)
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
                var path = FullName![(SmbShare.FullName!.Length + 1)..];
                (containers, elements) = await SmbShare.ListFolder(this, path, token);
            }
            catch { }

            _containers = containers.AsReadOnly();
            _elements = elements.AsReadOnly();

            if (_items != null)
            {
                foreach (var item in _items)
                {
                    item.Destroy();
                }
            }

            _items = _containers.Cast<IItem>().Concat(_elements).ToList().AsReadOnly();
            await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty, token);
        }

        public async Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default)
        {
            if (_items == null) await RefreshAsync(token);
            return _items;
        }
        public async Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default)
        {
            if (_containers == null) await RefreshAsync(token);
            return _containers;
        }
        public async Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default)
        {
            if (_elements == null) await RefreshAsync(token);
            return _elements;
        }
        public Task<bool> CanOpenAsync() => Task.FromResult(true);

        public void Destroy() => IsDestroyed = true;

        public void Unload()
        {
            _items = null;
            _containers = null;
            _elements = null;
        }
    }
}