namespace FileTime.Server.Common.Connections.SignalR;

public interface ISignalRClient
{
    Task RemoveTrackedItem(int itemId);
}