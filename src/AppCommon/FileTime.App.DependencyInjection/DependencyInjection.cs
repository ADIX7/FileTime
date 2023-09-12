using FileTime.App.Core;
using FileTime.App.Core.Models;
using FileTime.App.Core.Services;
using FileTime.App.Core.Services.Persistence;
using FileTime.App.Database;
using FileTime.Core;
using FileTime.Providers.Local;
using FileTime.Providers.LocalAdmin;
using FileTime.Providers.Remote;
using FileTime.Tools.VirtualDiskSources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.App.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection RegisterDefaultServices(IConfigurationRoot configuration, IServiceCollection? serviceCollection = null)
    {
        serviceCollection ??= new ServiceCollection();

        serviceCollection.TryAddSingleton<IApplicationSettings, ApplicationSettings>();
        serviceCollection.TryAddSingleton<ITabPersistenceService, TabPersistenceService>();
        serviceCollection.AddSingleton<IExitHandler, ITabPersistenceService>(sp => sp.GetRequiredService<ITabPersistenceService>());
        serviceCollection.AddSingleton<IStartupHandler, ITabPersistenceService>(sp => sp.GetRequiredService<ITabPersistenceService>());

        return serviceCollection
            .AddCoreDependencies()
            .AddDatabase()
            .AddAppCoreDependencies(configuration)
            .AddLocalProviderServices()
            .AddLocalAdminProviderServices(configuration)
            .AddRemoteProviderServices()
            .AddVirtualDisk();
    }
}