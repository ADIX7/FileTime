using System;
using System.IO;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.GuiApp.Configuration;
using FileTime.GuiApp.Logging;
using FileTime.GuiApp.Services;
using FileTime.GuiApp.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;

namespace FileTime.GuiApp;

public static class Startup
{
    internal static IServiceCollection AddViewModels(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<MainWindowViewModel>()
            .AddSingleton<GuiAppState>()
            .AddSingleton<IAppState, GuiAppState>(s => s.GetRequiredService<GuiAppState>())
            .AddSingleton<IGuiAppState, GuiAppState>(s => s.GetRequiredService<GuiAppState>());
    }
    internal static IServiceCollection RegisterServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IRxSchedulerService, AvaloniaRxSchedulerService>()
            .AddSingleton<IKeyInputHandlerService, KeyInputHandlerService>()
            .AddSingleton<IDefaultModeKeyInputHandler, DefaultModeKeyInputHandler>()
            .AddSingleton<IKeyboardConfigurationService, KeyboardConfigurationService>()
            .AddSingleton<IRapidTravelModeKeyInputHandler, RapidTravelModeKeyInputHandler>()
            .AddSingleton<IStartupHandler, RootDriveInfoService>()
            .AddSingleton<LifecycleService>()
            .AddSingleton<IModalService, ModalService>();
    }

    internal static IServiceCollection RegisterLogging(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true)
        );
    }

    internal static IServiceCollection AddConfiguration(this IServiceCollection serviceCollection)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(MainConfiguration.Configuration)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{Program.EnvironmentName}.json", true)
            .Build();

        return serviceCollection
            .Configure<ProgramsConfiguration>(configuration.GetSection(SectionNames.ProgramsSectionName))
            .Configure<KeyBindingConfiguration>(configuration.GetSection(SectionNames.KeybindingSectionName))
            .AddSingleton<IConfiguration>(configuration);
    }

    internal static IServiceProvider InitSerilog(this IServiceProvider serviceProvider)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(serviceProvider.GetService<IConfiguration>())
            .Enrich.FromLogContext()
            .WriteTo.File(
                Path.Combine(Program.AppDataRoot, "logs", "appLog.log"),
                fileSizeLimitBytes: 10 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                rollingInterval: RollingInterval.Day)
            .WriteTo.MessageBoxSink(serviceProvider)
            .CreateLogger();

        return serviceProvider;
    }

    internal static LoggerConfiguration MessageBoxSink(
        this LoggerSinkConfiguration loggerConfiguration,
        IServiceProvider serviceProvider)
    {
        return loggerConfiguration.Sink(serviceProvider.GetService<ToastMessageSink>());
    }
}