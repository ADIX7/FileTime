using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.Providers.Remote;

public static class Startup
{
    public static IServiceCollection AddRemoteProviderServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<IRemoteContentProvider, RemoteContentProvider>();
        serviceCollection.TryAddTransient<IRemoteItemCreator, RemoteItemCreator>();
        serviceCollection.TryAddTransient<IRemoteItemDeleter, RemoteItemDeleter>();
        serviceCollection.TryAddTransient<IRemoteItemMover, RemoteItemMover>();
        return serviceCollection;
    }
}