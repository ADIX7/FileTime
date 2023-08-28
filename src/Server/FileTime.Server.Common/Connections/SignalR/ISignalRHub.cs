using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Serialization;
using FileTime.Core.Timeline;

namespace FileTime.Server.Common.Connections.SignalR;

public interface ISignalRHub
{
    Task SetClientIdentifier(string providerName);
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
    Task<string> GetNativePathAsync(string contentProviderId, string fullNamePath);
    Task<ISerialized> GetItemByNativePathAsync(
        string contentProviderId,
        NativePath nativePath,
        PointInTime pointInTime,
        bool forceResolve,
        AbsolutePathType forceResolvePathType,
        ItemInitializationSettings itemInitializationSettings);

    Task<SerializedAbsolutePath[]> GetChildren(
        string contentProviderId,
        string fullName);
}