using System.Text;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Core.Timeline;
using FileTime.Server.Common;
using FileTime.Server.Common.Connections.SignalR;
using FileTime.Server.Common.ContentAccess;
using Microsoft.AspNetCore.SignalR;

namespace FileTime.Server.Web;

public class ConnectionHub : Hub<ISignalRClient>, ISignalRHub
{
    private readonly IContentProviderRegistry _contentProviderRegistry;
    private readonly IContentAccessorFactory _contentAccessorFactory;
    private readonly IApplicationStopper _applicationStopper;
    private readonly IContentAccessManager _contentAccessManager;

    public ConnectionHub(
        IContentProviderRegistry contentProviderRegistry,
        IContentAccessorFactory contentAccessorFactory,
        IApplicationStopper applicationStopper,
        IContentAccessManager contentAccessManager)
    {
        _contentProviderRegistry = contentProviderRegistry;
        _contentAccessorFactory = contentAccessorFactory;
        _applicationStopper = applicationStopper;
        _contentAccessManager = contentAccessManager;
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

    public async Task DeleteItemAsync(string contentProviderId, string fullName)
    {
        var contentProvider = _contentProviderRegistry.ContentProviders.First(p => p.Name == contentProviderId);
        var itemDeleter = _contentAccessorFactory.GetItemDeleter(contentProvider);
        await itemDeleter.DeleteAsync(contentProvider, new FullName(fullName));
    }

    public async Task MoveItemAsync(string contentProviderId, string fullName, string newPath)
    {
        var contentProvider = _contentProviderRegistry.ContentProviders.First(p => p.Name == contentProviderId);
        var itemDeleter = _contentAccessorFactory.GetItemMover(contentProvider);
        await itemDeleter.RenameAsync(contentProvider, new FullName(fullName), new FullName(newPath));
    }

    public async Task InitializeRemoteWriter(string contentProviderId, string transactionId, string nativePath)
    {
        var contentProvider = _contentProviderRegistry.ContentProviders.First(p => p.Name == contentProviderId);
        var item = await contentProvider.GetItemByNativePathAsync(new NativePath(nativePath), PointInTime.Present);
        if (item is not IElement element)
            throw new FileNotFoundException("Item is not an element", nativePath);

        var contentWriter = await _contentAccessorFactory.GetContentWriterFactory(contentProvider).CreateContentWriterAsync(element);
        _contentAccessManager.AddContentWriter(transactionId, contentWriter);
    }

    public async Task WriteBytesAsync(string transactionId, string data, int index) 
        => await _contentAccessManager.GetContentWriter(transactionId).WriteBytesAsync(Encoding.UTF8.GetBytes(data), index == -1 ? null : index);

    public async Task FlushWriterAsync(string transactionId)
        => await _contentAccessManager.GetContentWriter(transactionId).FlushAsync();


    public Task CloseWriterAsync(string transactionId)
    {
        _contentAccessManager.GetContentWriter(transactionId).Dispose();
        _contentAccessManager.RemoveContentWriter(transactionId);
        return Task.CompletedTask;
    }
}