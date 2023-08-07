using FileTime.App.CommandPalette;
using FileTime.App.ContainerSizeScanner;
using FileTime.App.DependencyInjection;
using FileTime.App.FrequencyNavigation;
using FileTime.App.Search;
using FileTime.ConsoleUI.App;
using FileTime.Providers.Local;
using FileTime.Server.Common;
using FileTime.Tools.Compression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FileTime.ConsoleUI;

public class DI
{
    public static IServiceProvider ServiceProvider { get; set; } = null!;

    public static void Initialize(IConfigurationRoot configuration)
        => ServiceProvider = DependencyInjection
                .RegisterDefaultServices(configuration: configuration)
                .AddConsoleServices()
                .AddLocalProviderServices()
                .AddServerCoreServices()
                .AddFrequencyNavigation()
                .AddCommandPalette()
                .AddContainerSizeScanner()
                .AddSearch()
                .AddCompression()
                .AddLogging(loggingBuilder => loggingBuilder.AddSerilog())
                .BuildServiceProvider();
}