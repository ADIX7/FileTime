using FileTime.App.Core;
using FileTime.App.Core.Models;
using FileTime.App.Core.Services;
using FileTime.App.Core.Services.Persistence;
using FileTime.Core.Command;
using FileTime.Core.Command.Copy;
using FileTime.Core.Command.CreateContainer;
using FileTime.Core.Command.CreateElement;
using FileTime.Core.Command.Delete;
using FileTime.Core.CommandHandlers;
using FileTime.Core.ContentAccess;
using FileTime.Core.Services;
using FileTime.Core.Timeline;
using FileTime.Providers.Local;
using FileTime.Providers.LocalAdmin;
using FileTime.Providers.Remote;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.App.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection RegisterDefaultServices(IConfigurationRoot configuration, IServiceCollection? serviceCollection = null)
    {
        serviceCollection ??= new ServiceCollection();

        serviceCollection.TryAddSingleton<ICommandScheduler, CommandScheduler>();
        serviceCollection.TryAddSingleton<ITimelessContentProvider, TimelessContentProvider>();
        serviceCollection.TryAddSingleton<ICommandRunner, CommandRunner>();
        serviceCollection.TryAddSingleton<IContentAccessorFactory, ContentAccessorFactory>();
        serviceCollection.TryAddSingleton<IContentProviderRegistry, ContentProviderRegistry>();
        //TODO: check local/remote context 
        serviceCollection.TryAddSingleton<ILocalCommandExecutor, LocalCommandExecutor>();
        serviceCollection.TryAddSingleton<ICommandSchedulerNotifier, LocalCommandSchedulerNotifier>();

        serviceCollection.TryAddSingleton<IApplicationSettings, ApplicationSettings>();
        serviceCollection.TryAddSingleton<ITabPersistenceService, TabPersistenceService>();
        serviceCollection.TryAddTransient<ITab, Tab>();
        serviceCollection.TryAddSingleton<ITabEvents, TabEvents>();
        serviceCollection.AddSingleton<IExitHandler, ITabPersistenceService>(sp => sp.GetRequiredService<ITabPersistenceService>());
        serviceCollection.AddSingleton<IStartupHandler, ITabPersistenceService>(sp => sp.GetRequiredService<ITabPersistenceService>());

        return serviceCollection
            .AddCoreAppServices()
            .AddLocalProviderServices()
            .AddLocalAdminProviderServices(configuration)
            .AddRemoteProviderServices()
            .RegisterCommands()
            .AddDefaultCommandHandlers();
    }

    private static IServiceCollection RegisterCommands(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddCommands()
            .AddTransient<CreateContainerCommand>()
            .AddTransient<CreateElementCommand>()
            .AddTransient<DeleteCommand>();
}