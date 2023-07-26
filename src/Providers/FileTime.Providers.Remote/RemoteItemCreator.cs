using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Server.Common;

namespace FileTime.Providers.Remote;

public class RemoteItemCreator : 
    ItemCreatorBase<IRemoteContentProvider>, 
    IRemoteItemCreator
{
    private IRemoteConnection _remoteConnection = null!;
    private string _remoteContentProviderId = null!;
    public void Init(IRemoteConnection remoteConnection, string remoteContentProviderId)
    {
        _remoteConnection = remoteConnection;
        _remoteContentProviderId = remoteContentProviderId;
    }

    public override async Task CreateContainerAsync(IRemoteContentProvider contentProvider, FullName fullName) 
        => await _remoteConnection.CreateContainerAsync(_remoteContentProviderId, fullName);

    public override async Task CreateElementAsync(IRemoteContentProvider contentProvider, FullName fullName) 
        => await _remoteConnection.CreateElementAsync(_remoteContentProviderId, fullName);
}