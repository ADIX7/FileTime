using System.Text;
using FileTime.Core.Models;
using InitableService;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Serilog;
using TypedSignalR.Client;

namespace FileTime.Server.Common.Connections.SignalR;

public class SignalRConnection : IRemoteConnection, IAsyncInitable<string>
{
    private static readonly Dictionary<string, SignalRConnection> Connections = new();
    private static readonly object ConnectionsLock = new();

    private string _baseUrl = null!;
    private HubConnection _connection = null!;
    private ISignalRHub _client = null!;

    public async Task InitAsync(string baseUrl)
    {
        _baseUrl = baseUrl;

        var connectionBuilder = new HubConnectionBuilder()
            .ConfigureLogging(logging => { logging.AddSerilog(); })
            .WithUrl(_baseUrl);

        _connection = connectionBuilder.Build();
        await _connection.StartAsync();
        _client = _connection.CreateHubProxy<ISignalRHub>();
    }

    public static async Task<SignalRConnection> GetOrCreateForAsync(string baseUrl)
    {
        SignalRConnection? connection;
        lock (ConnectionsLock)
        {
            if (Connections.TryGetValue(baseUrl, out connection))
            {
                if (connection._connection.State != HubConnectionState.Disconnected)
                {
                    return connection;
                }

                Connections.Remove(baseUrl);
            }

            connection = new SignalRConnection();
            Connections.Add(baseUrl, connection);
        }

        await connection.InitAsync(baseUrl);
        return connection;
    }

    public async Task Exit()
        => await _client.Exit();

    public async Task CreateContainerAsync(string contentProviderId, FullName fullName)
        => await _client.CreateContainerAsync(contentProviderId, fullName.Path);

    public async Task CreateElementAsync(string contentProviderId, FullName fullName)
        => await _client.CreateElementAsync(contentProviderId, fullName.Path);

    public async Task DeleteItemAsync(string contentProviderId, FullName fullName)
        => await _client.DeleteItemAsync(contentProviderId, fullName.Path);

    public async Task MoveItemAsync(string contentProviderId, FullName fullName, FullName newPath)
        => await _client.MoveItemAsync(contentProviderId, fullName.Path, newPath.Path);

    public async Task WriteBytesAsync(string transactionId, byte[] data, int? index, CancellationToken cancellationToken = default)
        => await _client.WriteBytesAsync(transactionId, Convert.ToBase64String(data), index ?? -1);

    public async Task FlushWriterAsync(string transactionId, CancellationToken cancellationToken = default)
        => await _client.FlushWriterAsync(transactionId);

    public async Task InitializeRemoteWriter(string contentProviderId, string transactionId, NativePath nativePath)
        => await _client.InitializeRemoteWriter(contentProviderId, transactionId, nativePath.Path);

    public async Task CloseWriterAsync(string transactionId)
        => await _client.CloseWriterAsync(transactionId);
}