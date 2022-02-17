using System;
using System.IO;
using System.Runtime.InteropServices;
using FileTime.Avalonia.Application;
using FileTime.Avalonia.Configuration;
using FileTime.Avalonia.IconProviders;
using FileTime.Avalonia.Logging;
using FileTime.Avalonia.Services;
using FileTime.Avalonia.ViewModels;
using FileTime.Core.Command;
using FileTime.Core.Interactions;
using FileTime.Core.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;

namespace FileTime.Avalonia
{
    internal static class Startup
    {
        internal static IServiceCollection AddViewModels(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddTransient<MainPageViewModel>()
                .AddSingleton<IInputInterface, BasicInputHandler>();
        }
        internal static IServiceCollection AddServices(this IServiceCollection serviceCollection)
        {
            serviceCollection = serviceCollection
                .AddSingleton<AppState>()
                .AddSingleton<ItemNameConverterService>()
                .AddSingleton<StatePersistenceService>()
                .AddSingleton<CommandHandlerService>()
                .AddSingleton<KeyboardConfigurationService>()
                .AddSingleton<KeyInputHandlerService>()
                .AddSingleton<IDialogService, DialogService>()
                .AddSingleton(new PersistenceSettings(Program.AppDataRoot))
                .AddSingleton<ProgramsService>()
                .AddSingleton<ToastMessageSink>()
                .AddSingleton<IIconProvider, MaterialIconProvider>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                serviceCollection.AddSingleton<IContextMenuProvider, WindowsContextMenuProvider>();
            }
            else
            {
                serviceCollection.AddSingleton<IContextMenuProvider, LinuxContextMenuProvider>();
            }

            return serviceCollection;
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
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Program.EnvironmentName}.json", true, true)
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

        public static LoggerConfiguration MessageBoxSink(
            this LoggerSinkConfiguration loggerConfiguration,
            IServiceProvider serviceProvider)
        {
            return loggerConfiguration.Sink(serviceProvider.GetService<ToastMessageSink>());
        }
    }
}
