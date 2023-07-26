using FileTime.Core.Models;

namespace FileTime.Server.Common;

public interface IRemoteConnection
{
    Task Exit();
    Task CreateContainerAsync(string contentProviderId, FullName fullName);
    Task CreateElementAsync(string contentProviderId, FullName fullName);
    Task DeleteItemAsync(string contentProviderId, FullName fullName);
}