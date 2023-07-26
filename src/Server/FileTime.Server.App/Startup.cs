using FileTime.Providers.LocalAdmin;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Server.App;

public static class Startup
{
    public static IServiceCollection AddServerServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IAdminElevationManager, DummyAdminElevationManager>();
        return serviceCollection;
    }
}