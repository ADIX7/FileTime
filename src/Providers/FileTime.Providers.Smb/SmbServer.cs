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
        private ISMBClient? _client;
        private readonly IInputInterface _inputInterface;

        public IReadOnlyList<IItem> Items
        {
            get
            {
                if (_shares == null) Refresh();
                return _shares!;
            }
        }

        public IReadOnlyList<IContainer> Containers
        {
            get
            {
                if (_shares == null) Refresh();
                return _shares!;
            }

            private set => _shares = value;
        }

        public IReadOnlyList<IElement> Elements { get; } = new List<IElement>().AsReadOnly();

        public string Name { get; }

        public string? FullName { get; }

        public bool IsHidden => false;

        public SmbContentProvider Provider { get; }

        IContentProvider IItem.Provider => Provider;

        public event EventHandler? Refreshed;

        public SmbServer(string path, SmbContentProvider contentProvider, IInputInterface inputInterface)
        {
            _inputInterface = inputInterface;

            Provider = contentProvider;
            FullName = Name = path;
        }

        public IContainer CreateContainer(string name)
        {
            throw new NotSupportedException();
        }

        public IElement CreateElement(string name)
        {
            throw new NotSupportedException();
        }

        public void Delete()
        {
        }

        public IItem? GetByPath(string path)
        {
            throw new NotImplementedException();
        }

        public IContainer? GetParent() => Provider;

        public bool IsExists(string name)
        {
            throw new NotImplementedException();
        }

        public void Refresh()
        {
            ISMBClient client = GetSmbClient();

            List<string> shares =  new List<string>(); //client.ListShares(out var status);

            _shares = shares.ConvertAll(s => new SmbShare(s, Provider, this, GetSmbClient)).AsReadOnly();
            Refreshed?.Invoke(this, EventArgs.Empty);
        }

        private ISMBClient GetSmbClient()
        {
            if (_client == null)
            {
                _client = new SMB2Client();
                if (_client.Connect(Name[2..], SMBTransportType.DirectTCPTransport))
                {
                    if (_username == null && _password == null)
                    {
                        var inputs = _inputInterface.ReadInputs(
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