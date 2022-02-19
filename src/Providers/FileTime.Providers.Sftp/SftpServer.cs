using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace FileTime.Providers.Sftp
{
    public class SftpServer : AbstractContainer<SftpContentProvider>
    {
        internal const int MAXRETRIES = 5;
        private bool _reenterCredentials;
        private SftpClient? _client;
        private readonly SftpClientContext _sftpClientContext;
        private bool _refreshingClient;
        private readonly object _clientGuard = new();
        private readonly IInputInterface _inputInterface;

        public string? Username { get; private set; }
        public string? Password { get; private set; }
        public override bool IsExists => true;

        public SftpServer(string name, SftpContentProvider sftpContentProvider, IInputInterface inputInterface, string? username = null, string? password = null)
            : base(sftpContentProvider, sftpContentProvider, name)
        {
            _inputInterface = inputInterface;
            _sftpClientContext = new SftpClientContext(GetClient, () => { });
            Username = username;
            Password = password;

            Name = name;
            NativePath = FullName = sftpContentProvider.Protocol + Constants.SeparatorChar + name;

            CanDelete = SupportsDelete.True;
        }

        public override async Task<IEnumerable<IItem>> RefreshItems(CancellationToken token = default) => await ListDirectory(this, "");

        public override Task<IContainer> CloneAsync() => Task.FromResult((IContainer)this);

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

        public override Task Rename(string newName)
        {
            throw new NotImplementedException();
        }

        public override void Unload()
        {
            base.Unload();

            lock (_clientGuard)
            {
                _client?.Disconnect();
                _client = null;
            }
        }

        private async Task<SftpClient> GetClient(int maxRetries = MAXRETRIES)
        {
            bool isClientNull;
            lock (_clientGuard)
            {
                isClientNull = _client == null;
            }

            for (int reTries = 0; isClientNull; reTries++)
            {
                if (!await RefreshSftpClient())
                {
                    await Task.Delay(1);
                }

                lock (_clientGuard)
                {
                    isClientNull = _client == null;
                }

                if (reTries >= maxRetries)
                {
                    throw new Exception($"Could not connect to server {Name} after {reTries} retry");
                }
            }
            return _client!;
        }

        private async Task<bool> RefreshSftpClient()
        {
            lock (_clientGuard)
            {
                if (_refreshingClient) return false;
                _refreshingClient = true;
            }
            try
            {
                if (_reenterCredentials || Username == null || Password == null)
                {
                    var inputs = await _inputInterface.ReadInputs(
                        new InputElement[]
                        {
                                InputElement.ForText($"Username for '{Name}'", Username ?? ""),
                                InputElement.ForPassword($"Password for '{Name}'", Password ?? "")
                        });

                    Username = inputs[0];
                    Password = inputs[1];
                }

                var client = new SftpClient(Name, Username, Password);
                try
                {
                    client.Connect();
                }
                catch (SshAuthenticationException)
                {
                    _reenterCredentials = true;
                }
                catch
                {
                    throw;
                }

                lock (_clientGuard)
                {
                    _client = client;
                }
            }
            finally
            {
                lock (_clientGuard)
                {
                    _refreshingClient = false;
                }
            }

            return true;
        }

        public async Task<IEnumerable<IItem>> ListDirectory(IContainer parent, string path)
        {
            return await _sftpClientContext.RunWithSftpClientAsync(client =>
            {
                var containers = new List<IContainer>();
                var elements = new List<IElement>();

                foreach (var file in client.ListDirectory(path))
                {
                    if (file.IsDirectory)
                    {
                        var container = new SftpFolder(Provider, this, parent, file.Name);
                        containers.Add(container);
                    }
                }

                return containers.Cast<IItem>().Concat(elements);
            });
        }
    }
}