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

        public Task<IContainer> CreateContainer(string name)
        {
            throw new NotImplementedException();
        }

        public Task<IElement> CreateElement(string name)
        {
            throw new NotImplementedException();
        }

        public Task Delete(bool hardDelete = false)
        {
            throw new NotImplementedException();
        }

        public IContainer? GetParent() => _parent;

        public Task<IContainer> Clone() => Task.FromResult((IContainer)this);

        public Task<bool> IsExists(string name)
        {
            throw new NotImplementedException();
        }

        public async Task RefreshAsync(CancellationToken token = default)
        {
            var containers = new List<IContainer>();
            var elements = new List<IElement>();

            try
            {
                (containers, elements) = await ListFolder(this, Name, string.Empty, token);
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

        public async Task<(List<IContainer> containers, List<IElement> elements)> ListFolder(IContainer parent, string shareName, string folderName, CancellationToken token = default)
        {
            return await _smbClientContext.RunWithSmbClientAsync(client =>
            {
                var containers = new List<IContainer>();
                var elements = new List<IElement>();
                NTStatus status = NTStatus.STATUS_DATA_ERROR;
                ISMBFileStore fileStore = client.TreeConnect(shareName, out status);
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
                                    containers.Add(new SmbFolder(fileDirectoryInformation.FileName, Provider, this, parent));
                                }
                                else
                                {
                                    elements.Add(new SmbFile(fileDirectoryInformation.FileName, Provider, parent));
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

        public Task Rename(string newName) => throw new NotSupportedException();
        public Task<bool> CanOpen() => Task.FromResult(true);

        public void Destroy() { }

        public void Unload()
        {
            _items = null;
            _containers = null;
            _elements = null;
        }
    }
}