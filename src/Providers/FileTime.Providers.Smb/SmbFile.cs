using FileTime.Core.Models;
using FileTime.Core.Providers;
using SMBLibrary;

namespace FileTime.Providers.Smb
{
    public class SmbFile : IElement
    {
        private readonly IContainer _parent;
        private readonly SmbClientContext _smbClientContext;
        private readonly SmbShare _smbShare;

        public bool IsSpecial => false;

        public string Name { get; }

        public string? FullName { get; }
        public string? NativePath { get; }

        public bool IsHidden => false;
        public SupportsDelete CanDelete => SupportsDelete.True;
        public bool CanRename => true;

        public IContentProvider Provider { get; }

        public bool IsDestroyed { get; private set; }
        //TODO: implement
        public bool IsExists => true;

        public SmbFile(string name, SmbContentProvider provider, SmbShare smbShare, IContainer parent, SmbClientContext smbClientContext)
        {
            Provider = provider;
            _parent = parent;
            _smbClientContext = smbClientContext;
            _smbShare = smbShare;

            Name = name;
            FullName = parent.FullName + Constants.SeparatorChar + Name;
            NativePath = parent.NativePath + SmbContentProvider.GetNativePathSeparator() + name;
        }

        public async Task Delete(bool hardDelete = false)
        {
            await _smbClientContext.RunWithSmbClientAsync(client =>
            {
                var fileStore = _smbShare.TreeConnect(client, out var status);
                status = fileStore.CreateFile(
                    out object fileHandle,
                    out FileStatus fileStatus,
                    GetPathFromShare(),
                    AccessMask.GENERIC_WRITE | AccessMask.DELETE | AccessMask.SYNCHRONIZE,
                    SMBLibrary.FileAttributes.Normal,
                    ShareAccess.None,
                    CreateDisposition.FILE_OPEN,
                    CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT,
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

        public string GetPrimaryAttributeText()
        {
            return "";
        }

        public IContainer? GetParent() => _parent;
        public Task<string> GetContent(CancellationToken token = default) => Task.FromResult("NotImplemented");
        public Task<long> GetElementSize(CancellationToken token = default) => Task.FromResult(-1L);

        public void Destroy() => IsDestroyed = true;

        public async Task<IContentReader> GetContentReaderAsync()
        {
            return await _smbClientContext.RunWithSmbClientAsync(client =>
            {
                NTStatus status = NTStatus.STATUS_DATA_ERROR;
                var fileStore = _smbShare.TreeConnect(client, out status);

                if (status != NTStatus.STATUS_SUCCESS)
                {
                    throw new Exception($"Could not open file {NativePath} for read.");
                }

                status = fileStore.CreateFile(
                    out object fileHandle,
                    out FileStatus fileStatus,
                    GetPathFromShare(),
                    AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE,
                    SMBLibrary.FileAttributes.Normal,
                    ShareAccess.Read,
                    CreateDisposition.FILE_OPEN,
                    CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT,
                    null);

                if (status != NTStatus.STATUS_SUCCESS)
                {
                    throw new Exception($"Could not open file {NativePath} for read.");
                }

                return new SmbContentReader(fileStore, fileHandle, client);
            });
        }

        public async Task<IContentWriter> GetContentWriterAsync()
        {
            return await _smbClientContext.RunWithSmbClientAsync(client =>
            {
                NTStatus status = NTStatus.STATUS_DATA_ERROR;
                var fileStore = _smbShare.TreeConnect(client, out status);

                if (status != NTStatus.STATUS_SUCCESS)
                {
                    throw new Exception($"Could not open file {NativePath} for write.");
                }

                status = fileStore.CreateFile(
                    out object fileHandle,
                    out FileStatus fileStatus,
                    GetPathFromShare(),
                    AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE,
                    SMBLibrary.FileAttributes.Normal,
                    ShareAccess.None,
                    CreateDisposition.FILE_OPEN_IF,
                    CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT,
                    null);

                if (status != NTStatus.STATUS_SUCCESS)
                {
                    throw new Exception($"Could not open file {NativePath} for write.");
                }

                return new SmbContentWriter(fileStore, fileHandle, client);
            });
        }

        private string GetPathFromShare() => FullName![(_smbShare.FullName!.Length + 1)..].Replace("/", "\\");
    }
}