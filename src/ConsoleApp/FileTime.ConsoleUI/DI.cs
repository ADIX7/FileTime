using FileTime.App.CommandPalette;
using FileTime.App.ContainerSizeScanner;
using FileTime.App.DependencyInjection;
using FileTime.App.FrequencyNavigation;
using FileTime.App.Search;
using FileTime.ConsoleUI.App;
using FileTime.ConsoleUI.App.Services;
using FileTime.Providers.Local;
using FileTime.Server.Common;
using FileTime.Tools.Compression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TerminalUI.DependencyInjection;

namespace FileTime.ConsoleUI;

public static class DI
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public static IServiceProvider Initialize(IConfigurationRoot configuration)
        => ServiceProvider = DependencyInjection
            .RegisterDefaultServices(configuration: configuration)
            .AddConsoleServices(configuration)
            .AddConsoleViews()
            .AddTerminalUi()
            .AddLocalProviderServices()
            .AddServerCoreServices()
            .AddFrequencyNavigation()
            .AddCommandPalette()
            .AddContainerSizeScanner()
            .AddSearch()
            .AddCompression()
            .SetupLogging()
            .AddLogging(loggingBuilder => loggingBuilder.AddSerilog())
            .AddConsoleDriver()
            .AddTheme()
            .BuildServiceProvider();


    private static IServiceCollection SetupLogging(this IServiceCollection serviceCollection) =>
        serviceCollection.AddSerilog(
            (serviceProvider, loggerConfiguration) =>
            {
                loggerConfiguration
#if DEBUG || VERBOSE_LOGGING
                    .MinimumLevel.Verbose()
#endif
                    .ReadFrom.Configuration(serviceProvider.GetRequiredService<IConfiguration>())
                    .Enrich.FromLogContext()
                    .WriteTo.File(
                        Path.Combine(Program.AppDataRoot, "logs", "appLog.log"),
                        fileSizeLimitBytes: 10 * 1024 * 1024,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true)
                    .WriteTo.Sink(serviceProvider.GetRequiredService<CustomLoggerSink>());
            }
        );
}