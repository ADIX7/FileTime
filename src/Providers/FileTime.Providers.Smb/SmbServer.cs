using System.Net;
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
        private string? _username;
        private string? _password;

        private IReadOnlyList<IContainer>? _shares;
        private IReadOnlyList<IItem>? _items;
        private readonly IReadOnlyList<IElement>? _elements = new List<IElement>().AsReadOnly();
        private ISMBClient? _client;
        private readonly object _clientGuard = new object();
        private bool _refreshingClient;
        private readonly IInputInterface _inputInterface;
        private readonly SmbClientContext _smbClientContext;

        public string Name { get; }

        public string? FullName { get; }

        public bool IsHidden => false;
        public bool IsLoaded => _items != null;

        public SmbContentProvider Provider { get; }

        IContentProvider IItem.Provider => Provider;
        public SupportsDelete CanDelete => SupportsDelete.True;
        public bool CanRename => false;
        public IReadOnlyList<Exception> Exceptions { get; } = new List<Exception>().AsReadOnly();

        public AsyncEventHandler Refreshed { get; } = new();

        public bool SupportsDirectoryLevelSoftDelete => false;

        public bool IsDestroyed => false;

        public SmbServer(string path, SmbContentProvider contentProvider, IInputInterface inputInterface)
        {
            _inputInterface = inputInterface;
            _smbClientContext = new SmbClientContext(GetSmbClient, DisposeSmbClient);

            Provider = contentProvider;
            FullName = Name = path;
        }

        public async Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default)
        {
            if (_shares == null) await RefreshAsync();
            return _shares;
        }
        public async Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default)
        {
            if (_shares == null) await RefreshAsync();
            return _shares;
        }
        public Task<IReadOnlyList<IElement>?> GetElements(CancellationToken token = default)
        {
            return Task.FromResult(_elements);
        }

        public Task<IContainer> CreateContainer(string name)
        {
            throw new NotSupportedException();
        }

        public Task<IElement> CreateElement(string name)
        {
            throw new NotSupportedException();
        }

        public Task Delete(bool hardDelete = false)
        {
            return Task.CompletedTask;
        }

        public Task<IItem?> GetByPath(string path, bool acceptDeepestMatch = false)
        {
            throw new NotImplementedException();
        }

        public IContainer? GetParent() => Provider;

        public Task<bool> IsExists(string name)
        {
            throw new NotImplementedException();
        }

        public async Task RefreshAsync(CancellationToken token = default)
        {
            List<string> shares = await _smbClientContext.RunWithSmbClientAsync((client) => client.ListShares(out var status));

            _shares = shares.ConvertAll(s => new SmbShare(s, Provider, this, _smbClientContext)).AsReadOnly();
            _items = _shares.Cast<IItem>().ToList().AsReadOnly();
            await Refreshed.InvokeAsync(this, AsyncEventArgs.Empty, token);
        }

        public Task<IContainer> Clone() => Task.FromResult((IContainer)this);

        private void DisposeSmbClient()
        {
            lock (_clientGuard)
            {
                _client = null;
            }
        }

        private async Task<ISMBClient> GetSmbClient()
        {
            bool isClientNull;
            lock (_clientGuard)
            {
                isClientNull = _client == null;
            }

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
                var couldParse = IPAddress.TryParse(Name[2..], out var ipAddress);
                var client = new SMB2Client();
                var connected = couldParse
                    ? client.Connect(ipAddress, SMBTransportType.DirectTCPTransport)
                    : client.Connect(Name[2..], SMBTransportType.DirectTCPTransport);

                if (connected)
                {
                    if (_username == null && _password == null)
                    {
                        var inputs = await _inputInterface.ReadInputs(
                            new InputElement[]
                            {
                                new InputElement($"Username for '{Name}'", InputType.Text),
                                new InputElement($"Password for '{Name}'", InputType.Password)
                            });

                        _username = inputs[0];
                        _password = inputs[1];
                    }

                    if (client.Login(string.Empty, _username, _password) != NTStatus.STATUS_SUCCESS)
                    {
                        _username = null;
                        _password = null;
                    }
                    else
                    {
                        lock (_clientGuard)
                        {
                            _client = client;
                        }
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
        public Task<bool> CanOpen() => Task.FromResult(true);

        public void Destroy() { }

        public void Unload() { }
    }
}