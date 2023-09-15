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
    Task CloseWriterAsync(string transactionId);
    Task<NativePath> GetNativePathAsync(string contentProviderId, FullName fullName);
    
    Task FlushAsync(string transactionId);
    Task<int> ReadAsync(string transactionId, byte[] buffer, int offset, int count);
    Task<long> SeekAsync(string transactionId, long offset, SeekOrigin origin);
    Task SetLengthAsync(string transactionId, long value);
    Task WriteAsync(string transactionId, byte[] buffer, int offset, int count);
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