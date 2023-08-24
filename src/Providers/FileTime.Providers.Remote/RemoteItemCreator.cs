using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using InitableService;

namespace FileTime.Providers.Remote;

public class RemoteItemCreator :
    ItemCreatorBase<IRemoteContentProvider>,
    IInitable<IRemoteContentProvider>
{
    private IRemoteContentProvider _remoteContentProvider = null!;

    public void Init(IRemoteContentProvider remoteConnection) => _remoteContentProvider = remoteConnection;

    public override async Task CreateContainerAsync(IRemoteContentProvider contentProvider, FullName fullName)
        => await (await _remoteContentProvider.GetRemoteConnectionAsync())
            .CreateContainerAsync(_remoteContentProvider.RemoteProviderName, fullName);

    public override async Task CreateElementAsync(IRemoteContentProvider contentProvider, FullName fullName)
        => await (await _remoteContentProvider.GetRemoteConnectionAsync())
            .CreateElementAsync(_remoteContentProvider.RemoteProviderName, fullName);
}