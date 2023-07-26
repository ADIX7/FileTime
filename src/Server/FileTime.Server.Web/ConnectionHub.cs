using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Server.Common;
using FileTime.Server.Common.Connections.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace FileTime.Server.Web;

public class ConnectionHub : Hub<ISignalRClient>, ISignalRHub
{
    private readonly IContentProviderRegistry _contentProviderRegistry;
    private readonly IContentAccessorFactory _contentAccessorFactory;
    private readonly IApplicationStopper _applicationStopper;

    public ConnectionHub(
        IContentProviderRegistry contentProviderRegistry,
        IContentAccessorFactory contentAccessorFactory,
        IApplicationStopper applicationStopper)
    {
        _contentProviderRegistry = contentProviderRegistry;
        _contentAccessorFactory = contentAccessorFactory;
        _applicationStopper = applicationStopper;
    }
    
    public Task Exit()
    {
        _applicationStopper.Stop();
        return Task.CompletedTask;
    }

    public async Task CreateContainerAsync(string contentProviderId, string fullName)
    {
        //TODO handle no content provider with id
        var contentProvider = _contentProviderRegistry.ContentProviders.First(p => p.Name == contentProviderId);
        var itemCreator = _contentAccessorFactory.GetItemCreator(contentProvider);
        await itemCreator.CreateContainerAsync(contentProvider, new FullName(fullName));
    }

    public async Task CreateElementAsync(string contentProviderId, string fullName)
    {
        var contentProvider = _contentProviderRegistry.ContentProviders.First(p => p.Name == contentProviderId);
        var itemCreator = _contentAccessorFactory.GetItemCreator(contentProvider);
        await itemCreator.CreateElementAsync(contentProvider, new FullName(fullName));
    }
}