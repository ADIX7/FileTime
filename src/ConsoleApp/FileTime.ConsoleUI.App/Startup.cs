using FileTime.App.Core.Configuration;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.ConsoleUI.App.Configuration;
using FileTime.ConsoleUI.App.Controls;
using FileTime.ConsoleUI.App.KeyInputHandling;
using FileTime.ConsoleUI.App.Services;
using FileTime.ConsoleUI.App.Styling;
using FileTime.ConsoleUI.App.UserCommand;
using FileTime.Core.Interactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.ConsoleUI.App;

public class StartupHandler : IStartupHandler
{
    public StartupHandler(IIdentifiableUserCommandService identifiableUserCommandService)
    {
        identifiableUserCommandService.AddIdentifiableUserCommand(NextPreviewUserCommand.Instance);
        identifiableUserCommandService.AddIdentifiableUserCommand(PreviousPreviewUserCommand.Instance);
    }

    public Task InitAsync() => Task.CompletedTask;
}

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
        services.AddSingleton<IUserCommandHandler, ConsoleUserCommandHandler>();
        services.AddSingleton<IStartupHandler, StartupHandler>();
        services.TryAddSingleton<IIconProvider, NerdFontIconProvider>();
        services.TryAddSingleton<IThemeProvider, ThemeProvider>();

        services.Configure<ConsoleApplicationConfiguration>(configuration);
        services.Configure<StyleConfigurationRoot>(configuration.GetSection("Style"));
        return services;
    }

    public static IServiceCollection AddSettings(this IServiceCollection services)
    {
        services.TryAddSingleton(new TabPersistenceSettings {LoadState = false, SaveState = false});
        return services;
    }

    public static IServiceCollection AddConsoleViews(this IServiceCollection services)
    {
        services.TryAddSingleton<MainWindow>();
        services.TryAddSingleton<CommandPalette>();
        services.TryAddSingleton<Dialogs>();
        services.TryAddSingleton<Timeline>();
        services.TryAddSingleton<FrequencyNavigation>();
        services.TryAddSingleton<ItemPreviews>();
        return services;
    }
}