namespace FileTime.Server.Common.Connections.SignalR;

public interface ISignalRHub
{
    Task Exit();
    Task CreateContainerAsync(string contentProviderId, string fullName);
    Task CreateElementAsync(string contentProviderId, string fullName);
}