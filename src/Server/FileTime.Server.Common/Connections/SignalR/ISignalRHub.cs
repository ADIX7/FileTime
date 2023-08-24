namespace FileTime.Server.Common.Connections.SignalR;

public interface ISignalRHub
{
    Task Exit();
    Task CreateContainerAsync(string contentProviderId, string fullName);
    Task CreateElementAsync(string contentProviderId, string fullName);
    Task DeleteItemAsync(string contentProviderId, string fullName);
    Task MoveItemAsync(string contentProviderId, string fullName, string newPath);

    //TODO: CancellationToken https://github.com/nenoNaninu/TypedSignalR.Client/issues/120
    Task FlushWriterAsync(string transactionId);
    Task InitializeRemoteWriter(string contentProviderId, string transactionId, string nativePath);

    //TODO: CancellationToken https://github.com/nenoNaninu/TypedSignalR.Client/issues/120
    Task WriteBytesAsync(string transactionId, string data, int index);
    Task CloseWriterAsync(string transactionId);
    Task<string?> GetNativePathAsync(string fullNamePath);
}