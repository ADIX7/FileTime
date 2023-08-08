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

namespace FileTime.ConsoleUI;

public static class DI
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public static void Initialize(IConfigurationRoot configuration, IServiceCollection serviceCollection)
        => ServiceProvider = DependencyInjection
            .RegisterDefaultServices(configuration: configuration, serviceCollection: serviceCollection)
            .AddConsoleServices()
            .AddLocalProviderServices()
            .AddServerCoreServices()
            .AddFrequencyNavigation()
            .AddCommandPalette()
            .AddContainerSizeScanner()
            .AddSearch()
            .AddCompression()
            .SetupLogging()
            .AddLogging(loggingBuilder => loggingBuilder.AddSerilog())
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
                        rollingInterval: RollingInterval.Day,
                        rollOnFileSizeLimit: true)
                    .WriteTo.Sink(serviceProvider.GetRequiredService<CustomLoggerSink>());
            }
        );
}