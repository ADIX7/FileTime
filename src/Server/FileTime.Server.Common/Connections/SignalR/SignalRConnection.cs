using FileTime.Core.Models;
using InitableService;
using Microsoft.AspNetCore.SignalR.Client;
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

        _connection = new HubConnectionBuilder()
            .WithUrl(_baseUrl)
            .Build();
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
}