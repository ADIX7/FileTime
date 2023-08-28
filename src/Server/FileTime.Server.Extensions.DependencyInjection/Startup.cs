using FileTime.Server.Common;
using FileTime.Server.Common.Connections.SignalR;
using FileTime.Server.Common.ContentAccess;
using FileTime.Server.Common.ItemTracker;
using FileTime.Server.Tracker.ItemTracker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.Server;

public static class Startup
{
    public static IServiceCollection AddServerCoreServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<SignalRConnection>();
        serviceCollection.TryAddSingleton<IApplicationStopper, ApplicationStopper>();
        serviceCollection.TryAddSingleton<IContentAccessManager, ContentAccessManager>();
        return serviceCollection.AddRemoteTrackerServices();
    }
    private static IServiceCollection AddRemoteTrackerServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<IItemTrackerRegistry, ItemTrackerRegistry>();
        return serviceCollection;
    }
}