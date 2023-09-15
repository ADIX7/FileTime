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
    Task InitializeRemoteWriter(string contentProviderId, string transactionId, string nativePath);

    Task CloseWriterAsync(string transactionId);
    Task<string> GetNativePathAsync(string contentProviderId, string fullNamePath);

    Task FlushAsync(string transactionId);
    Task<string> ReadAsync(string transactionId, int dataLength);
    Task<long> SeekAsync(string transactionId, long offset, SeekOrigin origin);
    Task SetLengthAsync(string transactionId, long value);
    Task WriteAsync(string transactionId, string data);
    Task<bool> CanReadAsync(string transactionId);
    Task<bool> CanSeekAsync(string transactionId);
    Task<bool> CanWriteAsync(string transactionId);
    Task<long> GetLengthAsync(string transactionId);
    Task<long> GetPositionAsync(string transactionId);
    Task SetPositionAsync(string transactionId, long position);

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