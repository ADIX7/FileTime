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
        private readonly IInputInterface _inputInterface;

        public string Name { get; }

        public string? FullName { get; }

        public bool IsHidden => false;

        public SmbContentProvider Provider { get; }

        IContentProvider IItem.Provider => Provider;

        public AsyncEventHandler Refreshed { get; } = new();

        public SmbServer(string path, SmbContentProvider contentProvider, IInputInterface inputInterface)
        {
            _inputInterface = inputInterface;

            Provider = contentProvider;
            FullName = Name = path;
        }

        public async Task<IReadOnlyList<IItem>?> GetItems(CancellationToken token = default)
        {
            if (_shares == null) await Refresh();
            return _shares;
        }
        public async Task<IReadOnlyList<IContainer>?> GetContainers(CancellationToken token = default)
        {
            if (_shares == null) await Refresh();
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

        public Task Delete()
        {
            return Task.CompletedTask;
        }

        public Task<IItem?> GetByPath(string path)
        {
            throw new NotImplementedException();
        }

        public IContainer? GetParent() => Provider;

        public Task<bool> IsExists(string name)
        {
            throw new NotImplementedException();
        }

        public async Task Refresh()
        {
            ISMBClient client = await GetSmbClient();

            List<string> shares = client.ListShares(out var status);

            _shares = shares.ConvertAll(s => new SmbShare(s, Provider, this, GetSmbClient)).AsReadOnly();
            _items = _shares.Cast<IItem>().ToList().AsReadOnly();
            await Refreshed?.InvokeAsync(this, AsyncEventArgs.Empty);
        }

        private async Task<ISMBClient> GetSmbClient()
        {
            if (_client == null)
            {
                var couldParse = IPAddress.TryParse(Name[2..], out var ipAddress);
                _client = new SMB2Client();
                var connected = couldParse
                    ? _client.Connect(ipAddress, SMBTransportType.DirectTCPTransport)
                    : _client.Connect(Name[2..], SMBTransportType.DirectTCPTransport);

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

                    if (_client.Login(string.Empty, _username, _password) != NTStatus.STATUS_SUCCESS)
                    {
                        _username = null;
                        _password = null;
                    }
                }
            }
            return _client;
        }
    }
}