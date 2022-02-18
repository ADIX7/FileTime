using System.Net;
using System.Runtime.InteropServices;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using SMBLibrary;
using SMBLibrary.Client;

namespace FileTime.Providers.Smb
{
    public class SmbServer : AbstractContainer<SmbContentProvider>, IContainer
    {
        internal const int MAXRETRIES = 5;

        private bool _reenterCredentials;
        private ISMBClient? _client;
        private readonly object _clientGuard = new();
        private bool _refreshingClient;
        private readonly IInputInterface _inputInterface;
        private readonly SmbClientContext _smbClientContext;

        public string? Username { get; private set; }
        public string? Password { get; private set; }
        public override bool IsExists => true;

        public SmbServer(string name, SmbContentProvider contentProvider, IInputInterface inputInterface, string? username = null, string? password = null)
         : base(contentProvider, contentProvider, name)
        {
            _inputInterface = inputInterface;
            _smbClientContext = new SmbClientContext(GetSmbClient, DisposeSmbClient);
            Username = username;
            Password = password;
            CanDelete = SupportsDelete.True;

            FullName = contentProvider.Protocol + Name;
            NativePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "\\\\" + Name
                : contentProvider.Protocol + Name;
        }

        public override Task<IContainer> CreateContainerAsync(string name)
        {
            throw new NotSupportedException();
        }

        public override Task<IElement> CreateElementAsync(string name)
        {
            throw new NotSupportedException();
        }

        public override Task Delete(bool hardDelete = false)
        {
            return Task.CompletedTask;
        }

        async Task<IItem?> IContainer.GetByPath(string path, bool acceptDeepestMatch)
        {
            var paths = path.Split(Constants.SeparatorChar);

            var item = (await GetItems())!.FirstOrDefault(i => i.Name == paths[0]);

            if (paths.Length == 1)
            {
                return item;
            }

            if (item is IContainer container)
            {
                var result = await container.GetByPath(string.Join(Constants.SeparatorChar, paths.Skip(1)), acceptDeepestMatch);
                return result == null && acceptDeepestMatch ? this : result;
            }

            return null;
        }

        public override async Task<IEnumerable<IItem>> RefreshItems(CancellationToken token = default)
        {
            try
            {
                var shares = await _smbClientContext.RunWithSmbClientAsync((client) => client.ListShares(out var status), IsLoaded ? MAXRETRIES : 1);

                return shares.Select(s => new SmbShare(s, Provider, this, _smbClientContext));
            }
            catch (Exception e)
            {
                AddException(e);
            }

            return Enumerable.Empty<IItem>();
        }

        public override Task<IContainer> CloneAsync() => Task.FromResult((IContainer)this);

        private void DisposeSmbClient()
        {
            lock (_clientGuard)
            {
                _client = null;
            }
        }

        private async Task<ISMBClient> GetSmbClient(int maxRetries = MAXRETRIES)
        {
            bool isClientNull;
            lock (_clientGuard)
            {
                isClientNull = _client == null;
            }

            for (var reTries = 0; isClientNull; reTries++)
            {
                if (!await RefreshSmbClient())
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

        private async Task<bool> RefreshSmbClient()
        {
            lock (_clientGuard)
            {
                if (_refreshingClient) return false;
                _refreshingClient = true;
            }
            try
            {
                var couldParse = IPAddress.TryParse(Name, out var ipAddress);
                var client = new SMB2Client();
                var connected = couldParse
                    ? client.Connect(ipAddress, SMBTransportType.DirectTCPTransport)
                    : client.Connect(Name, SMBTransportType.DirectTCPTransport);

                if (connected)
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

                    if (client.Login(string.Empty, Username, Password) != NTStatus.STATUS_SUCCESS)
                    {
                        _reenterCredentials = true;
                    }
                    else
                    {
                        _reenterCredentials = false;
                        lock (_clientGuard)
                        {
                            _client = client;
                        }

                        await Provider.SaveServers();
                    }
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

        public override Task Rename(string newName) => throw new NotSupportedException();
        public override Task<bool> CanOpenAsync() => Task.FromResult(true);
    }
}