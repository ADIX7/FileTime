using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.Providers.Remote;

public class RemoteItemDeleter : IItemDeleter<IRemoteContentProvider>, IInitable<IRemoteContentProvider>
{
    private IRemoteContentProvider _remoteContentProvider = null!;

    public void Init(IRemoteContentProvider remoteConnection) => _remoteContentProvider = remoteConnection;

    public async Task DeleteAsync(IRemoteContentProvider contentProvider, FullName fullName)
        => await (await _remoteContentProvider.GetRemoteConnectionAsync())
            .DeleteItemAsync(_remoteContentProvider.RemoteProviderName, fullName);
}