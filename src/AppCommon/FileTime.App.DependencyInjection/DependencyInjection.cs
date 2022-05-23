using FileTime.App.Core;
using FileTime.Core.Command;
using FileTime.Core.Services;
using FileTime.Core.Timeline;
using FileTime.Providers.Local;
using Microsoft.Extensions.DependencyInjection;
using ICommandExecutor = FileTime.Core.Timeline.ICommandExecutor;

namespace FileTime.App.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection RegisterDefaultServices(IServiceCollection? serviceCollection = null)
    {
        serviceCollection ??= new ServiceCollection();

        return serviceCollection
            .AddSingleton<ICommandScheduler, CommandScheduler>()
            .AddSingleton<ITimelessContentProvider, TimelessContentProvider>()
            .AddSingleton<ICommandRunner, CommandRunner>()
            .AddTransient<ITab, Tab>()
            .AddTransient<ILocalCommandExecutor, LocalCommandExecutor>()
            .AddCoreAppServices()
            .AddLocalServices();
    }
}