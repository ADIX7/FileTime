using FileTime.Core.Models;
using FileTime.Core.Providers;
using SMBLibrary;

namespace FileTime.Providers.Smb
{
    public class SmbFolder : AbstractContainer<SmbContentProvider>
    {
        private readonly SmbClientContext _smbClientContext;

        public SmbShare SmbShare { get; }
        //TODO: implement
        public override bool IsExists => true;

        public SmbFolder(string name, SmbContentProvider contentProvider, SmbShare smbShare, IContainer parent, SmbClientContext smbClientContext)
         : base(contentProvider, parent, name)
        {
            _smbClientContext = smbClientContext;
            SmbShare = smbShare;
            NativePath = parent.NativePath + SmbContentProvider.GetNativePathSeparator() + name;
            CanDelete = SupportsDelete.True;
            CanRename = true;
        }

        public override async Task<IContainer> CreateContainerAsync(string name)
        {
            var path = FullName![(SmbShare.FullName!.Length + 1)..] + Constants.SeparatorChar + name;
            await SmbShare.CreateContainerWithPathAsync(path.Replace("/", "\\"));
            await RefreshAsync();

            return (await GetContainers())!.FirstOrDefault(e => e.Name == name)!;
        }

        public override async Task<IElement> CreateElementAsync(string name)
        {
            var path = FullName![(SmbShare.FullName!.Length + 1)..] + Constants.SeparatorChar + name;
            await SmbShare.CreateElementWithPathAsync(path.Replace("/", "\\"));
            await RefreshAsync();

            return (await GetElements())!.FirstOrDefault(e => e.Name == name)!;
        }

        public override Task<IContainer> CloneAsync() => Task.FromResult((IContainer)new SmbFolder(Name, Provider, SmbShare, GetParent(), _smbClientContext));

        public override async Task Delete(bool hardDelete = false)
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
        public override Task Rename(string newName)
        {
            throw new NotImplementedException();
        }

        public override async Task<IEnumerable<IItem>> RefreshItems(CancellationToken token = default)
        {
            try
            {
                var path = FullName![(SmbShare.FullName!.Length + 1)..];
                var (containers, elements) = await SmbShare.ListFolder(this, path, token);

                return containers.Cast<IItem>().Concat(elements);
            }
            catch { }

            return Enumerable.Empty<IItem>();
        }

        private string GetPathFromShare() => FullName![(SmbShare.FullName!.Length + 1)..].Replace("/", "\\");
    }
}