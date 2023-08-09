using FileTime.App.Core.Configuration;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.ConsoleUI.App.Configuration;
using FileTime.ConsoleUI.App.KeyInputHandling;
using FileTime.ConsoleUI.App.Services;
using FileTime.Core.Interactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TerminalUI;
using TerminalUI.ConsoleDrivers;

namespace FileTime.ConsoleUI.App;

public static class Startup
{
    public static IServiceCollection AddConsoleServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.TryAddSingleton<IApplication, App>();
        services.TryAddSingleton<MainWindow>();
        services.TryAddSingleton<IConsoleAppState, ConsoleAppState>();
        services.TryAddSingleton<IAppState>(sp => sp.GetRequiredService<IConsoleAppState>());
        services.TryAddSingleton<IUserCommunicationService, ConsoleUserCommunicationService>();
        services.TryAddSingleton<IKeyInputHandlerService, KeyInputHandlerService>();
        services.TryAddSingleton<IAppKeyService<ConsoleKey>, ConsoleAppKeyService>();
        services.TryAddSingleton<ISystemClipboardService, ConsoleSystemClipboardService>();
        services.AddSingleton<CustomLoggerSink>();
        services.TryAddSingleton(new ApplicationConfiguration(true));

        services.Configure<ConsoleApplicationConfiguration>(configuration);

        services.TryAddSingleton<IApplicationContext>(sp
            => new ApplicationContext
            {
                ConsoleDriver = sp.GetRequiredService<IConsoleDriver>()
            }
        );
        return services;
    }
}