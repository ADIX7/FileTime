namespace FileTime.Server.Common.Connections.SignalR;

public interface ISignalRHub
{
    Task Exit();
    Task CreateContainerAsync(string contentProviderId, string fullName);
    Task CreateElementAsync(string contentProviderId, string fullName);
    Task DeleteItemAsync(string contentProviderId, string fullName);
    Task MoveItemAsync(string contentProviderId, string fullName, string newPath);
}