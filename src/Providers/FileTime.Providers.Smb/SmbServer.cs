using System.Net;
using System.Runtime.InteropServices;
using AsyncEvent;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Core.Providers;
using SMBLibrary;
using SMBLibrary.Client;

namespace FileTime.Providers.Smb
{
    public class SmbServer : IContainer
    {
        internal const int MAXRETRIES = 5;

        private bool _reenterCredentials;

        private IReadOnlyList<IContainer>? _shares;
        private IReadOnlyList<IItem>? _items;
        private readonly IReadOnlyList<IElement>? _elements = new List<IElement>().AsReadOnly();
        private ISMBClient? _client;
        private readonly object _clientGuard = new();
        private bool _refreshingClient;
        private readonly IInputInterface _inputInterface;
        private readonly SmbClientContext _smbClientContext;
        private readonly List<Exception> _exceptions = new();

        public string? Username { get; private set; }
        public string? Password { get; private set; }

        public string Name { get; }
        public string? FullName { get; }
        public string? NativePath { get; }

        public bool IsHidden => false;
        public bool IsLoaded => _items != null;

        public SmbContentProvider Provider { get; }

        IContentProvider IItem.Provider => Provider;
        public SupportsDelete CanDelete => SupportsDelete.True;
        public bool CanRename => false;
        public IReadOnlyList<Exception> Exceptions { get; }

        public AsyncEventHandler Refreshed { get; } = new();

        public bool SupportsDirectoryLevelSoftDelete => false;

        public bool IsDestroyed => false;

        public SmbServer(string path, SmbContentProvider contentProvider, IInputInterface inputInterface, string? username = null, string? password = null)
        {
            _inputInterface = inputInterface;
            _smbClientContext = new SmbClientContext(GetSmbClient, DisposeSmbClient);
            Exceptions = _exceptions.AsReadOnly();
            Username = username;
            Password = password;

            Provider = contentProvider;
            Name = path;
            FullName = contentProvider.Protocol + Name;
            NativePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "\\\\" + Name
                : contentProvider.Protocol + Name;
        }

        public async Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default)
        {
            if (_shares == null) await RefreshAsync(token);
            return _shares;
        }
        public async Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default)
        {
            if (_shares == null) await RefreshAsync(token);
            return _shares;
        }
        public Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default)
        {
            return Task.FromResult(_elements);
        }

        public Task<IContainer> CreateContainerAsync(string name)
        {
            throw new NotSupportedException();
        }

        public Task<IElement> CreateElementAsync(string name)
        {
            throw new NotSupportedException();
        }

        public Task Delete(bool hardDelete = false)
        {
            return Task.CompletedTask;
        }

        public async Task<IItem?> GetByPath(string path, bool acceptDeepestMatch = false)
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

        public IContainer? GetParent() => Provider;

        public Task<bool> IsExistsAsync(string name)
        {
            throw new NotImplementedException();
        }

        public async Task RefreshAsync(CancellationToken token = default)
        {
            try
            {
                _exceptions.Clear();
                List<string> shares = await _smbClientContext.RunWithSmbClientAsync((client) => client.ListShares(out var status), _shares == null ? 1 : MAXRETRIES);

                _shares = shares.ConvertAll(s => new SmbShare(s, Provider, this, _smbClientContext)).AsReadOnly();
                _items = _shares.Cast<IItem>().ToList().AsReadOnly();
                await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty, token);
            }
            catch (Exception e)
            {
                _exceptions.Add(e);
            }
        }

        public Task<IContainer> CloneAsync() => Task.FromResult((IContainer)this);

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

            int reTries = 0;
            while (isClientNull)
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
                reTries++;
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

        public Task Rename(string newName) => throw new NotSupportedException();
        public Task<bool> CanOpenAsync() => Task.FromResult(true);

        public void Destroy() { }

        public void Unload() { }
    }
}