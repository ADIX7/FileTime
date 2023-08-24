using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.Providers.Remote;

public class RemoteItemMover : IItemMover<IRemoteContentProvider>, IInitable<IRemoteContentProvider, string>
{
    private IRemoteContentProvider _remoteContentProvider = null!;
    private string _remoteContentProviderId = null!;

    public void Init(IRemoteContentProvider remoteConnection, string remoteContentProviderId)
    {
        _remoteContentProvider = remoteConnection;
        _remoteContentProviderId = remoteContentProviderId;
    }

    public async Task RenameAsync(IRemoteContentProvider contentProvider, FullName fullName, FullName newPath)
        => await (await _remoteContentProvider.GetRemoteConnectionAsync()).MoveItemAsync(_remoteContentProviderId, fullName, newPath);
}