using FileTime.Core.Models;
using FileTime.Core.Providers;
using SMBLibrary;
using SMBLibrary.Client;

namespace FileTime.Providers.Smb
{
    public class SmbShare : AbstractContainer<SmbContentProvider>
    {
        private readonly SmbClientContext _smbClientContext;

        public SmbShare(string name, SmbContentProvider contentProvider, IContainer parent, SmbClientContext smbClientContext)
         : base(contentProvider, parent, name)
        {
            _smbClientContext = smbClientContext;
            NativePath = parent.NativePath + SmbContentProvider.GetNativePathSeparator() + name;
            CanDelete = SupportsDelete.False;
        }

        public override async Task<IContainer> CreateContainerAsync(string name)
        {
            await CreateContainerWithPathAsync(name);
            await RefreshAsync();

            return (await GetContainers())!.FirstOrDefault(e => e.Name == name)!;
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

        public override async Task<IElement> CreateElementAsync(string name)
        {
            await CreateElementWithPathAsync(name);
            await RefreshAsync();

            return (await GetElements())!.FirstOrDefault(e => e.Name == name)!;
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

        public override Task Delete(bool hardDelete = false)
        {
            throw new NotSupportedException();
        }

        public override Task<IContainer> CloneAsync() => Task.FromResult((IContainer)this);

        public override async Task<IEnumerable<IItem>> RefreshItems(CancellationToken token = default)
        {
            try
            {
                var (containers, elements) = await ListFolder(this, string.Empty, token);

                return containers.Cast<IItem>().Concat(elements);
            }
            catch { }

            return Enumerable.Empty<IItem>();
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

        public override Task Rename(string newName) => throw new NotSupportedException();
    }
}