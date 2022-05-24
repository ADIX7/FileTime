using FileTime.App.Core;
using FileTime.Core.Command;
using FileTime.Core.Command.CreateContainer;
using FileTime.Core.ContentAccess;
using FileTime.Core.Services;
using FileTime.Core.Timeline;
using FileTime.Providers.Local;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.App.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection RegisterDefaultServices(IServiceCollection? serviceCollection = null)
    {
        serviceCollection ??= new ServiceCollection();

        serviceCollection.TryAddSingleton<ICommandScheduler, CommandScheduler>();
        serviceCollection.TryAddSingleton<ITimelessContentProvider, TimelessContentProvider>();
        serviceCollection.TryAddSingleton<ICommandRunner, CommandRunner>();
        serviceCollection.TryAddSingleton<IContentAccessorFactory, ContentAccessorFactory>();
        serviceCollection.TryAddSingleton<ITab, Tab>();
        serviceCollection.TryAddSingleton<ILocalCommandExecutor, LocalCommandExecutor>();

        return serviceCollection
            .AddCoreAppServices()
            .AddLocalServices()
            .RegisterCommands();
    }

    public static IServiceCollection RegisterCommands(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddTransient<CreateContainerCommand>();
    }
}