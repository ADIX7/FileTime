using FileTime.Core.ContentAccess;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Serialization;
using FileTime.Core.Timeline;
using FileTime.Server.Common;
using FileTime.Server.Common.Connections.SignalR;
using FileTime.Server.Common.ContentAccess;
using FileTime.Server.Common.ItemTracker;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Server.Web;

public class ConnectionHub : Hub<ISignalRClient>, ISignalRHub
{
    private readonly IContentProviderRegistry _contentProviderRegistry;
    private readonly IContentAccessorFactory _contentAccessorFactory;
    private readonly IApplicationStopper _applicationStopper;
    private readonly IContentAccessManager _contentAccessManager;
    private readonly IItemTrackerRegistry _itemTrackerRegistry;
    private readonly IServiceProvider _serviceProvider;

    private readonly Dictionary<int, string> _trackedItemIds = new();

    //TODO clean this sometimes
    private readonly Dictionary<string, string> _clientIdToConnectionId = new();

    public ConnectionHub(
        IContentProviderRegistry contentProviderRegistry,
        IContentAccessorFactory contentAccessorFactory,
        IApplicationStopper applicationStopper,
        IContentAccessManager contentAccessManager,
        IItemTrackerRegistry itemTrackerRegistry,
        IServiceProvider serviceProvider)
    {
        _contentProviderRegistry = contentProviderRegistry;
        _contentAccessorFactory = contentAccessorFactory;
        _applicationStopper = applicationStopper;
        _contentAccessManager = contentAccessManager;
        _itemTrackerRegistry = itemTrackerRegistry;
        _serviceProvider = serviceProvider;

        _itemTrackerRegistry.ItemRemoved += ItemTrackerRegistryOnItemRemoved;
    }

    private void ItemTrackerRegistryOnItemRemoved(int id)
    {
        if (_trackedItemIds.ContainsKey(id)) return;

        var clientId = _trackedItemIds[id];
        var connectionId = _clientIdToConnectionId[clientId];

        Clients.Client(connectionId).RemoveTrackedItem(id);
    }

    public Task SetClientIdentifier(string providerName)
    {
        _clientIdToConnectionId[providerName] = Context.ConnectionId;
        return Task.CompletedTask;
    }

    public Task Exit()
    {
        _applicationStopper.Stop();
        return Task.CompletedTask;
    }

    public async Task CreateContainerAsync(string contentProviderId, string fullName)
    {
        //TODO handle no content provider with id
        var contentProvider = GetContentProvider(contentProviderId);
        var itemCreator = _contentAccessorFactory.GetItemCreator(contentProvider);
        await itemCreator.CreateContainerAsync(contentProvider, new FullName(fullName));
    }

    public async Task CreateElementAsync(string contentProviderId, string fullName)
    {
        var contentProvider = GetContentProvider(contentProviderId);
        var itemCreator = _contentAccessorFactory.GetItemCreator(contentProvider);
        await itemCreator.CreateElementAsync(contentProvider, new FullName(fullName));
    }

    public async Task DeleteItemAsync(string contentProviderId, string fullName)
    {
        var contentProvider = GetContentProvider(contentProviderId);
        var itemDeleter = _contentAccessorFactory.GetItemDeleter(contentProvider);
        await itemDeleter.DeleteAsync(contentProvider, new FullName(fullName));
    }

    public async Task MoveItemAsync(string contentProviderId, string fullName, string newPath)
    {
        var contentProvider = GetContentProvider(contentProviderId);
        var itemDeleter = _contentAccessorFactory.GetItemMover(contentProvider);
        await itemDeleter.RenameAsync(contentProvider, new FullName(fullName), new FullName(newPath));
    }

    public async Task InitializeRemoteWriter(string contentProviderId, string transactionId, string nativePath)
    {
        var contentProvider = GetContentProvider(contentProviderId);
        var item = await contentProvider.GetItemByNativePathAsync(new NativePath(nativePath), PointInTime.Present);
        if (item is not IElement element)
            throw new FileNotFoundException("Item is not an element", nativePath);

        var contentWriter = await _contentAccessorFactory.GetContentWriterFactory(contentProvider).CreateContentWriterAsync(element);
        _contentAccessManager.AddContentWriter(transactionId, contentWriter);
    }

    public async Task WriteBytesAsync(string transactionId, string data, int index)
        => await _contentAccessManager.GetContentWriter(transactionId).WriteBytesAsync(Convert.FromBase64String(data), index == -1 ? null : index);

    public async Task FlushWriterAsync(string transactionId)
        => await _contentAccessManager.GetContentWriter(transactionId).FlushAsync();


    public Task CloseWriterAsync(string transactionId)
    {
        _contentAccessManager.GetContentWriter(transactionId).Dispose();
        _contentAccessManager.RemoveContentWriter(transactionId);
        return Task.CompletedTask;
    }

    public async Task<string> GetNativePathAsync(string contentProviderId, string fullNamePath)
    {
        var contentProvider = GetContentProvider(contentProviderId);
        return (await contentProvider.GetNativePathAsync(new FullName(fullNamePath))).Path;
    }

    public async Task<ISerialized> GetItemByNativePathAsync(
        string contentProviderId,
        NativePath nativePath,
        PointInTime pointInTime,
        bool forceResolve,
        AbsolutePathType forceResolvePathType,
        ItemInitializationSettings itemInitializationSettings)
    {
        var contentProvider = GetContentProvider(contentProviderId);
        var item = await contentProvider.GetItemByNativePathAsync(
            nativePath,
            pointInTime,
            forceResolve,
            forceResolvePathType,
            itemInitializationSettings
        );

        var id = _itemTrackerRegistry.Register(item);

        var serializerType = typeof(ISerializer<>).MakeGenericType(item.GetType());
        var serializer = (ISerializer) _serviceProvider.GetRequiredService(serializerType);
        var serializedObject = await serializer.SerializeAsync(id, item);

        return serializedObject;
    }

    public async Task<SerializedAbsolutePath[]> GetChildren(
        string contentProviderId,
        string fullName)
    {
        var contentProvider = GetContentProvider(contentProviderId);
        var item = await contentProvider.GetItemByFullNameAsync(
            new FullName(fullName),
            PointInTime.Present);

        if (item is IContainer container)
            return container.Items.Select(AbsolutePathSerializer.Serialize).ToArray();

        throw new NotSupportedException();
    }

    private IContentProvider GetContentProvider(string contentProviderId)
        => _contentProviderRegistry
            .ContentProviders
            .First(p => p.Name == contentProviderId);
}