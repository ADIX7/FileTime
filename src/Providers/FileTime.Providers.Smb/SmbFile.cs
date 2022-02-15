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

        public SmbFile(string name, SmbContentProvider provider, SmbShare smbShare, IContainer parent, SmbClientContext smbClientContext)
        {
            Name = name;
            FullName = parent.FullName + Constants.SeparatorChar + Name;
            NativePath = SmbContentProvider.GetNativePath(FullName);

            Provider = provider;
            _parent = parent;
            _smbClientContext = smbClientContext;
            _smbShare = smbShare;
        }

        public Task Delete(bool hardDelete = false)
        {
            throw new NotImplementedException();
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

                var path = NativePath!;
                path = path[(_parent.NativePath!.Length + 1)..];
                status = fileStore.CreateFile(out object fileHandle, out FileStatus fileStatus, path, AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE, SMBLibrary.FileAttributes.Normal, ShareAccess.Read, CreateDisposition.FILE_OPEN, CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null);

                if (status != NTStatus.STATUS_SUCCESS)
                {
                    throw new Exception($"Could not open file {NativePath} for read.");
                }

                return new SmbContentReader(fileStore, fileHandle, client);
            });
        }

        public Task<IContentWriter> GetContentWriterAsync()
        {
            throw new NotImplementedException();
        }
    }
}