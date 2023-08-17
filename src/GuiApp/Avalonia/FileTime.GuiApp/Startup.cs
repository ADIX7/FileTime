using System.IO;
using System.Runtime.InteropServices;
using Avalonia.Input;
using FileTime.App.Core.Configuration;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Interactions;
using FileTime.GuiApp.App.Configuration;
using FileTime.GuiApp.CustomImpl.ViewModels;
using FileTime.GuiApp.App.IconProviders;
using FileTime.GuiApp.App.Logging;
using FileTime.GuiApp.App.Services;
using FileTime.GuiApp.App.ViewModels;
using FileTime.Providers.Local;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;

namespace FileTime.GuiApp;

public static class Startup
{
    internal static IConfigurationRoot CreateConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(MainConfiguration.Configuration)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{Program.EnvironmentName}.json", true)
            .AddJsonFile("appsettings.Local.json", optional: true);

        var configurationDirectory = new DirectoryInfo(Path.Combine(Program.AppDataRoot, "config"));
        if (configurationDirectory.Exists)
        {
            foreach (var settingsFile in configurationDirectory.GetFiles("*.json"))
            {
                configurationBuilder.AddJsonFile(settingsFile.FullName, optional: true, reloadOnChange: true);
            }
        }

        return configurationBuilder.Build();
    }

    internal static IServiceCollection AddViewModels(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<MainWindowViewModel>();
        serviceCollection.TryAddSingleton<GuiAppState>();
        serviceCollection.TryAddSingleton<IAppState>(s => s.GetRequiredService<GuiAppState>());
        serviceCollection.TryAddSingleton<IGuiAppState>(s => s.GetRequiredService<GuiAppState>());
        return serviceCollection;
    }

    internal static IServiceCollection RegisterServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<IRxSchedulerService, AvaloniaRxSchedulerService>();
        serviceCollection.TryAddSingleton<IKeyInputHandlerService, KeyInputHandlerService>();
        serviceCollection.TryAddSingleton<IIconProvider, MaterialIconProvider>();
        serviceCollection.TryAddSingleton<IDialogService, DialogService>();
        serviceCollection.TryAddSingleton<SystemClipboardService>();
        serviceCollection.TryAddSingleton<ISystemClipboardService>(sp => sp.GetRequiredService<SystemClipboardService>());
        serviceCollection.TryAddSingleton<ToastMessageSink>();
        serviceCollection.TryAddSingleton<IUserCommunicationService>(s => s.GetRequiredService<IDialogService>());
        serviceCollection.TryAddSingleton<IAppKeyService<Key>, GuiAppKeyService>();
        serviceCollection.TryAddSingleton(new ApplicationConfiguration(false));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            serviceCollection
                .AddSingleton<IContextMenuProvider, WindowsContextMenuProvider>()
                .AddSingleton<IPlacesService, WindowsPlacesService>();
        }
        else
        {
            serviceCollection
                .AddSingleton<IContextMenuProvider, LinuxContextMenuProvider>()
                .AddSingleton<IPlacesService, LinuxPlacesService>();
        }

        return serviceCollection
            .AddSingleton<IExitHandler, RootDriveInfoService>()
            .AddSingleton<IStartupHandler>(sp => sp.GetRequiredService<IPlacesService>());
    }

    internal static IServiceCollection RegisterLogging(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSerilog(
            (serviceProvider, loggerConfiguration) =>
            {
                loggerConfiguration
#if DEBUG || VERBOSE_LOGGING
                    .MinimumLevel.Verbose()
#endif
#if DEBUG
                    .ReadFrom.Configuration(serviceProvider.GetRequiredService<IConfiguration>())
#endif
                    .Enrich.FromLogContext()
                    .WriteTo.File(
                        Path.Combine(Program.AppDataRoot, "logs", "appLog.log"),
                        fileSizeLimitBytes: 10 * 1024 * 1024,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true)
                    .WriteTo.Sink(serviceProvider.GetRequiredService<ToastMessageSink>());
            }
        );

        serviceCollection.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true)
        );

        return serviceCollection;
    }
}