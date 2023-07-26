using FileTime.Core.Models;
using InitableService;
using Microsoft.AspNetCore.SignalR.Client;
using TypedSignalR.Client;

namespace FileTime.Server.Common.Connections.SignalR;

public class SignalRConnection : IRemoteConnection, IAsyncInitable<string>
{
    private string _baseUrl = null!;
    private HubConnection _connection = null!;

    private ISignalRHub CreateClient() => _connection.CreateHubProxy<ISignalRHub>();

    public async Task InitAsync(string baseUrl)
    {
        _baseUrl = baseUrl;

        _connection = new HubConnectionBuilder()
            .WithUrl(_baseUrl)
            .Build();
        await _connection.StartAsync();
    }

    public async Task Exit()
    {
        var client = CreateClient();
        await client.Exit();
    }

    public async Task CreateContainerAsync(string contentProviderId, FullName fullName)
    {
        var client = CreateClient();
        await client.CreateContainerAsync(contentProviderId, fullName.Path);
    }

    public async Task CreateElementAsync(string contentProviderId, FullName fullName)
    {
        var client = CreateClient();
        await client.CreateElementAsync(contentProviderId, fullName.Path);
    }
}