using System.Runtime.Caching;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Serialization;
using FileTime.Core.Timeline;
using InitableService;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;
using TypedSignalR.Client;

namespace FileTime.Server.Common.Connections.SignalR;

public class SignalRConnection : IRemoteConnection, IAsyncInitable<string, string>
{
    private static readonly Dictionary<string, SignalRConnection> Connections = new();
    private static readonly object ConnectionsLock = new();

    private readonly MemoryCache _readCache = new(nameof(SignalRConnection));
    private string _baseUrl = null!;
    private HubConnection _connection = null!;
    private ISignalRHub _client = null!;

    public async Task InitAsync(string baseUrl, string providerName)
    {
        _baseUrl = baseUrl;

        var connectionBuilder = new HubConnectionBuilder()
            .ConfigureLogging(logging => { logging.AddSerilog(); })
            .WithUrl(_baseUrl);

        _connection = connectionBuilder.Build();
        await _connection.StartAsync();
        _client = _connection.CreateHubProxy<ISignalRHub>();
        await _client.SetClientIdentifier(providerName);
    }

    public static async ValueTask<IRemoteConnection> GetOrCreateForAsync(string baseUrl, string providerName)
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

        await connection.InitAsync(baseUrl, providerName);
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

    public async Task InitializeRemoteWriter(string contentProviderId, string transactionId, NativePath nativePath)
        => await _client.InitializeRemoteWriter(contentProviderId, transactionId, nativePath.Path);

    public async Task CloseWriterAsync(string transactionId)
        => await _client.CloseWriterAsync(transactionId);

    public async Task<NativePath> GetNativePathAsync(string contentProviderId, FullName fullName)
    {
        var path = await _client.GetNativePathAsync(contentProviderId, fullName.Path);
        return new NativePath(path);
    }

    public Task FlushAsync(string transactionId) => _client.FlushAsync(transactionId);

    public async Task<int> ReadAsync(string transactionId, byte[] buffer, int offset, int count)
    {
        var dataString = await _client.ReadAsync(transactionId, count);
        var data = GetDataFromString(dataString);

        _readCache.Add(new CacheItem(offset.ToString(), data), new CacheItemPolicy());
        
        data.CopyTo(buffer.AsSpan(offset, data.Length));
        
        return data.Length;
    }

    public Task<long> SeekAsync(string transactionId, long offset, SeekOrigin origin) => _client.SeekAsync(transactionId, offset, origin);

    public Task SetLengthAsync(string transactionId, long value) => _client.SetLengthAsync(transactionId, value);

    public Task WriteAsync(string transactionId, byte[] buffer, int offset, int count)
    {
        var data = GetStringFromData(buffer.AsSpan(offset, count));
        return _client.WriteAsync(transactionId, data);
    }

    public Task<bool> CanReadAsync(string transactionId) => _client.CanReadAsync(transactionId);

    public Task<bool> CanSeekAsync(string transactionId) => _client.CanSeekAsync(transactionId);

    public Task<bool> CanWriteAsync(string transactionId) => _client.CanWriteAsync(transactionId);

    public Task<long> GetLengthAsync(string transactionId) => _client.GetLengthAsync(transactionId);

    public Task<long> GetPositionAsync(string transactionId) => _client.GetPositionAsync(transactionId);

    public Task SetPositionAsync(string transactionId, long position) => _client.SetPositionAsync(transactionId, position);

    public async Task<ISerialized> GetItemByNativePathAsync(
        string contentProviderId,
        NativePath nativePath,
        PointInTime pointInTime,
        bool forceResolve,
        AbsolutePathType forceResolvePathType,
        ItemInitializationSettings itemInitializationSettings)
    {
        var item = await _client.GetItemByNativePathAsync(
            contentProviderId,
            nativePath, 
            pointInTime, 
            forceResolve, 
            forceResolvePathType, 
            itemInitializationSettings
        );

        return item;
    }

    public async Task<SerializedAbsolutePath[]> GetChildren(string contentProviderId, string fullName) 
        => await _client.GetChildren(contentProviderId, fullName);
    
    private static byte[] GetDataFromString(string data) => Convert.FromBase64String(data);
    private static string GetStringFromData(ReadOnlySpan<byte> data) => Convert.ToBase64String(data);
}