using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Server.Common;
using InitableService;

namespace FileTime.Providers.Remote;

public class RemoteItemCreator :
    ItemCreatorBase<IRemoteContentProvider>,
    IInitable<IRemoteContentProvider, string>
{
    private IRemoteContentProvider _remoteContentProvider = null!;
    private string _remoteContentProviderId = null!;

    public void Init(IRemoteContentProvider remoteConnection, string remoteContentProviderId)
    {
        _remoteContentProvider = remoteConnection;
        _remoteContentProviderId = remoteContentProviderId;
    }

    public override async Task CreateContainerAsync(IRemoteContentProvider contentProvider, FullName fullName)
        => await (await _remoteContentProvider.GetRemoteConnectionAsync()).CreateContainerAsync(_remoteContentProviderId, fullName);

    public override async Task CreateElementAsync(IRemoteContentProvider contentProvider, FullName fullName)
        => await (await _remoteContentProvider.GetRemoteConnectionAsync()).CreateElementAsync(_remoteContentProviderId, fullName);
}