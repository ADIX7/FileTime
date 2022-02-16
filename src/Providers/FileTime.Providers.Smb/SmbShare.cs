using AsyncEvent;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using SMBLibrary;
using SMBLibrary.Client;

namespace FileTime.Providers.Smb
{
    public class SmbShare : IContainer
    {
        private IReadOnlyList<IItem>? _items;
        private IReadOnlyList<IContainer>? _containers;
        private IReadOnlyList<IElement>? _elements;
        private readonly SmbClientContext _smbClientContext;
        private readonly IContainer? _parent;

        public string Name { get; }

        public string? FullName { get; }
        public string? NativePath { get; }

        public bool IsHidden => false;
        public bool IsLoaded => _items != null;

        public SmbContentProvider Provider { get; }
        IContentProvider IItem.Provider => Provider;
        public SupportsDelete CanDelete => SupportsDelete.False;
        public bool CanRename => false;

        public AsyncEventHandler Refreshed { get; } = new();
        public IReadOnlyList<Exception> Exceptions { get; } = new List<Exception>().AsReadOnly();

        public bool IsDestroyed => false;

        public bool SupportsDirectoryLevelSoftDelete => false;

        public SmbShare(string name, SmbContentProvider contentProvider, IContainer parent, SmbClientContext smbClientContext)
        {
            _parent = parent;
            _smbClientContext = smbClientContext;

            Name = name;
            FullName = parent?.FullName == null ? Name : parent.FullName + Constants.SeparatorChar + Name;
            NativePath = SmbContentProvider.GetNativePath(FullName);
            Provider = contentProvider;
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

        public async Task<IContainer> CreateContainerAsync(string name)
        {
            await CreateContainerWithPathAsync(name);
            await RefreshAsync();

            return _containers!.FirstOrDefault(e => e.Name == name)!;
        }
        internal async Task CreateContainerWithPathAsync(string path)
        {
            await _smbClientContext.RunWithSmbClientAsync(client =>
            {
                NTStatus status = NTStatus.STATUS_DATA_ERROR;
                var fileStore = TreeConnect(client, out status);

                if (status != NTStatus.STATUS_SUCCESS)
                {
                    throw new Exception($"Could not create directory {path}.");
                }

                status = fileStore.CreateFile(
                    out object fileHandle,
                    out FileStatus fileStatus,
                    path,
                    AccessMask.GENERIC_ALL,
                    SMBLibrary.FileAttributes.Directory,
                    ShareAccess.Read,
                    CreateDisposition.FILE_OPEN_IF,
                    CreateOptions.FILE_DIRECTORY_FILE,
                    null);

                if (status != NTStatus.STATUS_SUCCESS)
                {
                    throw new Exception($"Could not create directory {path}.");
                }

                fileStore.CloseFile(fileHandle);
                fileStore.Disconnect();
            });
        }

        public async Task<IElement> CreateElementAsync(string name)
        {
            await CreateElementWithPathAsync(name);
            await RefreshAsync();

            return _elements!.FirstOrDefault(e => e.Name == name)!;
        }
        internal async Task CreateElementWithPathAsync(string path)
        {
            await _smbClientContext.RunWithSmbClientAsync(client =>
            {
                NTStatus status = NTStatus.STATUS_DATA_ERROR;
                var fileStore = TreeConnect(client, out status);

                if (status != NTStatus.STATUS_SUCCESS)
                {
                    throw new Exception($"Could not create file {path}.");
                }

                status = fileStore.CreateFile(
                    out object fileHandle,
                    out FileStatus fileStatus,
                    path,
                    AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE,
                    SMBLibrary.FileAttributes.Normal,
                    ShareAccess.None,
                    CreateDisposition.FILE_CREATE,
                    CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT,
                    null);

                if (status != NTStatus.STATUS_SUCCESS)
                {
                    throw new Exception($"Could not create file {path}.");
                }

                fileStore.CloseFile(fileHandle);
                fileStore.Disconnect();
            });
        }

        public Task Delete(bool hardDelete = false)
        {
            throw new NotSupportedException();
        }

        public IContainer? GetParent() => _parent;

        public Task<IContainer> CloneAsync() => Task.FromResult((IContainer)this);

        public async Task<bool> IsExistsAsync(string name)
        {
            var items = await GetItems();
            return items?.Any(i => i.Name == name) ?? false;
        }

        public async Task RefreshAsync(CancellationToken token = default)
        {
            var containers = new List<IContainer>();
            var elements = new List<IElement>();

            try
            {
                (containers, elements) = await ListFolder(this, string.Empty, token);
            }
            catch { }

            if (_items != null)
            {
                foreach (var item in _items)
                {
                    item.Destroy();
                }
            }

            _containers = containers.AsReadOnly();
            _elements = elements.AsReadOnly();

            _items = _containers.Cast<IItem>().Concat(_elements).ToList().AsReadOnly();
            await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty, token);
        }

        public async Task<(List<IContainer> containers, List<IElement> elements)> ListFolder(IContainer parent, string folderName, CancellationToken token = default)
        {
            return await _smbClientContext.RunWithSmbClientAsync(client =>
            {
                var containers = new List<IContainer>();
                var elements = new List<IElement>();
                var status = NTStatus.STATUS_DATA_ERROR;
                var fileStore = TreeConnect(client, out status);

                if (status == NTStatus.STATUS_SUCCESS)
                {
                    status = fileStore.CreateFile(out object directoryHandle, out FileStatus fileStatus, folderName, AccessMask.GENERIC_READ, SMBLibrary.FileAttributes.Directory, ShareAccess.Read | ShareAccess.Write, CreateDisposition.FILE_OPEN, CreateOptions.FILE_DIRECTORY_FILE, null);
                    if (status == NTStatus.STATUS_SUCCESS)
                    {
                        status = fileStore.QueryDirectory(out List<QueryDirectoryFileInformation> fileList, directoryHandle, "*", FileInformationClass.FileDirectoryInformation);
                        status = fileStore.CloseFile(directoryHandle);

                        foreach (var item in fileList)
                        {
                            if (item is FileDirectoryInformation fileDirectoryInformation && fileDirectoryInformation.FileName != "." && fileDirectoryInformation.FileName != "..")
                            {
                                if ((fileDirectoryInformation.FileAttributes & SMBLibrary.FileAttributes.Directory) == SMBLibrary.FileAttributes.Directory)
                                {
                                    containers.Add(new SmbFolder(fileDirectoryInformation.FileName, Provider, this, parent, _smbClientContext));
                                }
                                else
                                {
                                    elements.Add(new SmbFile(fileDirectoryInformation.FileName, Provider, this, parent, _smbClientContext));
                                }
                            }
                        }
                    }
                }

                containers = containers.OrderBy(c => c.Name).ToList();
                elements = elements.OrderBy(e => e.Name).ToList();

                return (containers, elements);
            });
        }

        internal ISMBFileStore TreeConnect(ISMBClient client, out NTStatus status)
        {
            return client.TreeConnect(Name, out status);
        }

        public Task Rename(string newName) => throw new NotSupportedException();
        public Task<bool> CanOpenAsync() => Task.FromResult(true);

        public void Destroy() { }

        public void Unload()
        {
            _items = null;
            _containers = null;
            _elements = null;
        }
    }
}