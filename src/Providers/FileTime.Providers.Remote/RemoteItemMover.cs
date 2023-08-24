using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.Providers.Remote;

public class RemoteItemMover : IItemMover<IRemoteContentProvider>, IInitable<IRemoteContentProvider>
{
    private IRemoteContentProvider _remoteContentProvider = null!;

    public void Init(IRemoteContentProvider remoteConnection) => _remoteContentProvider = remoteConnection;

    public async Task RenameAsync(IRemoteContentProvider contentProvider, FullName fullName, FullName newPath)
        => await (await _remoteContentProvider.GetRemoteConnectionAsync())
            .MoveItemAsync(_remoteContentProvider.RemoteProviderName, fullName, newPath);
}