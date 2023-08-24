using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.Providers.Remote;

public class RemoteItemDeleter : IItemDeleter<IRemoteContentProvider>, IInitable<IRemoteContentProvider, string>
{
    private IRemoteContentProvider _remoteContentProvider = null!;
    private string _remoteContentProviderId = null!;

    public void Init(IRemoteContentProvider remoteConnection, string remoteContentProviderId)
    {
        _remoteContentProvider = remoteConnection;
        _remoteContentProviderId = remoteContentProviderId;
    }

    public async Task DeleteAsync(IRemoteContentProvider contentProvider, FullName fullName)
        => await (await _remoteContentProvider.GetRemoteConnectionAsync()).DeleteItemAsync(_remoteContentProviderId, fullName);
}