using FileTime.Core.Command;
using FileTime.Core.Command.Copy;
using FileTime.Core.Command.CreateContainer;
using FileTime.Core.Command.CreateElement;
using FileTime.Core.Command.Delete;
using FileTime.Core.Command.Move;
using FileTime.Core.CommandHandlers;
using FileTime.Core.ContentAccess;
using FileTime.Core.Models;
using FileTime.Core.Serialization;
using FileTime.Core.Serialization.Container;
using FileTime.Core.Services;
using FileTime.Core.Timeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.Core;

public static class Startup
{
    public static IServiceCollection AddCoreDependencies(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddCoreServices()
            .AddTimelineServices()
            .AddDefaultCommandHandlers()
            .AddCommands()
            .AddCommandFactories()
            .AddCommandServices()
            .AddContentAccessServices()
            .AddSerialization();

    private static IServiceCollection AddContentAccessServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<IContentAccessorFactory, ContentAccessorFactory>();
        serviceCollection.TryAddSingleton<IContentProviderRegistry, ContentProviderRegistry>();
        serviceCollection.TryAddSingleton<IRootContentProvider, RootContentProvider>();

        return serviceCollection;
    }

    private static IServiceCollection AddCommandServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<ICommandRunner, CommandRunner>();

        return serviceCollection;
    }

    private static IServiceCollection AddCoreServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddTransient<ITab, Tab>();
        serviceCollection.TryAddSingleton<ITabEvents, TabEvents>();

        return serviceCollection;
    }

    private static IServiceCollection AddTimelineServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<ICommandScheduler, CommandScheduler>();
        serviceCollection.TryAddSingleton<ITimelessContentProvider, TimelessContentProvider>();
        //TODO: check local/remote context 
        serviceCollection.TryAddSingleton<ILocalCommandExecutor, LocalCommandExecutor>();
        serviceCollection.TryAddSingleton<ICommandSchedulerNotifier, LocalCommandSchedulerNotifier>();

        return serviceCollection;
    }

    private static IServiceCollection AddCommands(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddTransient<CreateContainerCommand>()
            .AddTransient<CreateElementCommand>()
            .AddTransient<DeleteCommand>();

    private static IServiceCollection AddCommandFactories(this IServiceCollection serviceCollection) =>
        serviceCollection
            .AddSingleton<CopyCommandFactory>()
            .AddSingleton<CopyStrategyFactory>()
            .AddSingleton<MoveCommandFactory>();


    private static IServiceCollection AddSerialization(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<ISerializer<IContainer>, ContainerSerializer>();
        return serviceCollection;
    }

    private static IServiceCollection AddDefaultCommandHandlers(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddSingleton<ICommandHandler, StreamCopyCommandHandler>();
}