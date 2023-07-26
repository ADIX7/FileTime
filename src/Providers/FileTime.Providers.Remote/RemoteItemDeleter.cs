using FileTime.Core.Models;
using FileTime.Server.Common;

namespace FileTime.Providers.Remote;

public class RemoteItemDeleter : IRemoteItemDeleter
{
    private IRemoteConnection _remoteConnection = null!;
    private string _remoteContentProviderId = null!;
    public void Init(IRemoteConnection remoteConnection, string remoteContentProviderId)
    {
        _remoteConnection = remoteConnection;
        _remoteContentProviderId = remoteContentProviderId;
    }
    public async Task DeleteAsync(IRemoteContentProvider contentProvider, FullName fullName) 
        => await _remoteConnection.DeleteItemAsync(_remoteContentProviderId, fullName);
}