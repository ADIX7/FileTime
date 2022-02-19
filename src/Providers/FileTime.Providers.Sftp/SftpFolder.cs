using FileTime.Core.Models;
using FileTime.Core.Providers;

namespace FileTime.Providers.Sftp
{
    public class SftpFolder : AbstractContainer<SftpContentProvider>
    {
        private readonly SftpServer _server;
        public override bool IsExists => true;

        public SftpFolder(SftpContentProvider provider, SftpServer server, IContainer parent, string path) : base(provider, parent, path)
        {
            _server = server;
            NativePath = FullName;
        }

        public override Task<IContainer> CloneAsync()
        {
            return Task.FromResult((IContainer)new SftpFolder(Provider, _server, GetParent()!, Name));
        }

        public override Task<IContainer> CreateContainerAsync(string name)
        {
            throw new NotImplementedException();
        }

        public override Task<IElement> CreateElementAsync(string name)
        {
            throw new NotImplementedException();
        }

        public override Task Delete(bool hardDelete = false)
        {
            throw new NotImplementedException();
        }

        public override async Task<IEnumerable<IItem>> RefreshItems(CancellationToken token = default) => await _server.ListDirectory(this, FullName![(_server.FullName!.Length + 1)..]);

        public override Task Rename(string newName)
        {
            throw new NotImplementedException();
        }
    }
}