using FileTime.Core.Models;
using FileTime.Server.Common;

namespace FileTime.Providers.Remote;

public class RemoteItemMover : IRemoteItemMover
{

    private IRemoteConnection _remoteConnection = null!;
    private string _remoteContentProviderId = null!;
    public void Init(IRemoteConnection remoteConnection, string remoteContentProviderId)
    {
        _remoteConnection = remoteConnection;
        _remoteContentProviderId = remoteContentProviderId;
    }

    public async Task RenameAsync(IRemoteContentProvider contentProvider, FullName fullName, FullName newPath)
        => await _remoteConnection.MoveItemAsync(_remoteContentProviderId, fullName, newPath);
}