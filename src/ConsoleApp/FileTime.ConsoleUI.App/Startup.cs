using FileTime.App.Core.Configuration;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.ConsoleUI.App.Configuration;
using FileTime.ConsoleUI.App.Controls;
using FileTime.ConsoleUI.App.KeyInputHandling;
using FileTime.ConsoleUI.App.Services;
using FileTime.Core.Interactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.ConsoleUI.App;

public static class Startup
{
    public static IServiceCollection AddConsoleServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.TryAddSingleton<IApplication, App>();
        services.TryAddSingleton<IConsoleAppState, ConsoleAppState>();
        services.TryAddSingleton<IAppState>(sp => sp.GetRequiredService<IConsoleAppState>());
        services.TryAddSingleton<IKeyInputHandlerService, KeyInputHandlerService>();
        services.TryAddSingleton<IAppKeyService<ConsoleKey>, ConsoleAppKeyService>();
        services.TryAddSingleton<ISystemClipboardService, ConsoleSystemClipboardService>();
        services.AddSingleton<CustomLoggerSink>();
        services.TryAddSingleton(new ApplicationConfiguration(true));
        services.TryAddSingleton<IRootViewModel, RootViewModel>();
        services.TryAddSingleton<IDialogService, DialogService>();
        services.TryAddSingleton<IUserCommunicationService>(sp => sp.GetRequiredService<IDialogService>());

        services.Configure<ConsoleApplicationConfiguration>(configuration);
        return services;
    }

    public static IServiceCollection AddConsoleViews(this IServiceCollection services)
    {
        services.TryAddSingleton<MainWindow>();
        services.TryAddSingleton<CommandPalette>();
        services.TryAddSingleton<Dialogs>();
        return services;
    }
}