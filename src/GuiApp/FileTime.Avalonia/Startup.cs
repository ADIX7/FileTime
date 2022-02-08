using System.IO;
using System.Runtime.InteropServices;
using FileTime.Avalonia.Application;
using FileTime.Avalonia.Configuration;
using FileTime.Avalonia.IconProviders;
using FileTime.Avalonia.Services;
using FileTime.Avalonia.ViewModels;
using FileTime.Core.Command;
using FileTime.Core.Interactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FileTime.Avalonia
{
    internal static class Startup
    {
        internal static IServiceCollection AddViewModels(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<AppState>()
                .AddTransient<MainPageViewModel>()
                .AddSingleton<IInputInterface, BasicInputHandler>();
        }
        internal static IServiceCollection AddServices(this IServiceCollection serviceCollection)
        {
            serviceCollection = serviceCollection
                .AddSingleton<ItemNameConverterService>()
                .AddSingleton<StatePersistenceService>()
                .AddSingleton<CommandHandlerService>()
                .AddSingleton<KeyboardConfigurationService>()
                .AddSingleton<KeyInputHandlerService>()
                .AddSingleton<DialogService>()
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
        internal static IServiceCollection RegisterCommandHandlers(this IServiceCollection serviceCollection)
        {
            foreach (var commandHandler in Providers.Local.Startup.GetCommandHandlers())
            {
                serviceCollection.AddTransient(typeof(ICommandHandler), commandHandler);
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
                .Configure<KeyBindingConfiguration>(configuration.GetSection(MainConfiguration.KeybindingBaseConfigKey))
                .AddSingleton<IConfiguration>(configuration);
        }

        internal static IServiceCollection InitSerilog(this IServiceCollection serviceCollection)
        {
            using var serviceProvider = serviceCollection.BuildServiceProvider();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(serviceProvider.GetService<IConfiguration>())
                .Enrich.FromLogContext()
                .WriteTo.File(
                    Path.Combine(Program.AppDataRoot, "logs", "appLog.log"),
                    fileSizeLimitBytes: 10 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            return serviceCollection;
        }
    }
}
