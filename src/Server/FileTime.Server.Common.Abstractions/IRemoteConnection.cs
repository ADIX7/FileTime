using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Serialization;
using FileTime.Core.Timeline;

namespace FileTime.Server.Common;

public interface IRemoteConnection
{
    Task Exit();
    Task CreateContainerAsync(string contentProviderId, FullName fullName);
    Task CreateElementAsync(string contentProviderId, FullName fullName);
    Task DeleteItemAsync(string contentProviderId, FullName fullName);
    Task MoveItemAsync(string contentProviderId, FullName fullName, FullName newPath);
    Task InitializeRemoteWriter(string contentProviderId, string transactionId, NativePath nativePath);
    Task WriteBytesAsync(string transactionId, byte[] data, int? index, CancellationToken cancellationToken = default);
    Task FlushWriterAsync(string transactionId, CancellationToken cancellationToken = default);
    Task CloseWriterAsync(string transactionId);
    Task<NativePath> GetNativePathAsync(string contentProviderId, FullName fullName);

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