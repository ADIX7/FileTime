using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using SMBLibrary;

namespace FileTime.Providers.Smb
{
    public class SmbFolder : IContainer
    {
        private IReadOnlyList<IItem>? _items;
        private IReadOnlyList<IContainer>? _containers;
        private IReadOnlyList<IElement>? _elements;
        private readonly IContainer? _parent;
        private readonly SmbClientContext _smbClientContext;

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

        public SmbFolder(string name, SmbContentProvider contentProvider, SmbShare smbShare, IContainer parent, SmbClientContext smbClientContext)
        {
            _parent = parent;
            SmbShare = smbShare;

            Name = name;
            FullName = parent?.FullName == null ? Name : parent.FullName + Constants.SeparatorChar + Name;
            NativePath = SmbContentProvider.GetNativePath(FullName);
            Provider = contentProvider;
            _smbClientContext = smbClientContext;
        }

        public async Task<IContainer> CreateContainerAsync(string name)
        {
            var path = FullName![(SmbShare.FullName!.Length + 1)..] + Constants.SeparatorChar + name;
            await SmbShare.CreateContainerWithPathAsync(SmbContentProvider.GetNativePath(path));
            await RefreshAsync();

            return _containers!.FirstOrDefault(e => e.Name == name)!;
        }

        public async Task<IElement> CreateElementAsync(string name)
        {
            var path = FullName![(SmbShare.FullName!.Length + 1)..] + Constants.SeparatorChar + name;
            await SmbShare.CreateElementWithPathAsync(SmbContentProvider.GetNativePath(path));
            await RefreshAsync();

            return _elements!.FirstOrDefault(e => e.Name == name)!;
        }

        public Task<IContainer> CloneAsync() => Task.FromResult((IContainer)this);

        public IContainer? GetParent() => _parent;

        public async Task<bool> IsExistsAsync(string name)
        {
            var items = await GetItems();
            return items?.Any(i => i.Name == name) ?? false;
        }

        public async Task Delete(bool hardDelete = false)
        {
            await _smbClientContext.RunWithSmbClientAsync(client =>
            {
                var fileStore = SmbShare.TreeConnect(client, out var status);
                status = fileStore.CreateFile(
                    out object fileHandle,
                    out FileStatus fileStatus,
                    GetPathFromShare(),
                    AccessMask.GENERIC_WRITE | AccessMask.DELETE | AccessMask.SYNCHRONIZE,
                    SMBLibrary.FileAttributes.Normal,
                    ShareAccess.None,
                    CreateDisposition.FILE_OPEN,
                    CreateOptions.FILE_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT,
                    null);

                if (status == NTStatus.STATUS_SUCCESS)
                {
                    var fileDispositionInformation = new FileDispositionInformation
                    {
                        DeletePending = true
                    };
                    status = fileStore.SetFileInformation(fileHandle, fileDispositionInformation);
                    bool deleteSucceeded = status == NTStatus.STATUS_SUCCESS;
                    status = fileStore.CloseFile(fileHandle);
                }
                status = fileStore.Disconnect();
            });
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

        private string GetPathFromShare() => SmbContentProvider.GetNativePath(FullName![(SmbShare.FullName!.Length + 1)..]);
    }
}