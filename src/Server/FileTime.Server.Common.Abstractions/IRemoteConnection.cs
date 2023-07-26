using FileTime.Core.Models;

namespace FileTime.Server.Common;

public interface IRemoteConnection
{
    Task Exit();
    Task CreateContainerAsync(string contentProviderId, FullName fullName);
    Task CreateElementAsync(string contentProviderId, FullName fullName);
    Task DeleteItemAsync(string contentProviderId, FullName fullName);
    Task MoveItemAsync(string contentProviderId, FullName fullName, FullName newPath);
    Task InitializeRemoteWriter(string contentProviderId, string transactionId, NativePath nativePath);
    Task WriteBytesAsync(string transactionId, byte[] data, int? index);
    Task FlushWriterAsync(string transactionId);
    Task CloseWriterAsync(string transactionId);
}