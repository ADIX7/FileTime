using System.Linq;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FileTime.App.CommandPalette;
using FileTime.App.ContainerSizeScanner;
using FileTime.App.Core.Models;
using FileTime.App.DependencyInjection;
using FileTime.App.FrequencyNavigation;
using FileTime.App.Search;
using FileTime.Core.Models;
using FileTime.GuiApp.App;
using FileTime.GuiApp.App.Font;
using FileTime.GuiApp.App.ViewModels;
using FileTime.GuiApp.App.Views;
using FileTime.Server;
using FileTime.Tools.Compression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace FileTime.GuiApp;

public class Application : Avalonia.Application
{
    private static void InitializeApp()
    {
        Log.Logger.Information("App initialization starting...");
        var configuration = Startup.CreateConfiguration();
        var services = DependencyInjection
            .RegisterDefaultServices(configuration: configuration)
            .AddServerCoreServices()
            .AddFrequencyNavigation()
            .AddCommandPalette()
            .AddContainerSizeScanner()
            .AddSearch()
            .AddCompression()
            .ConfigureFont(configuration)
            .RegisterLogging()
            .RegisterGuiServices()
            .AddSettings()
            .AddViewModels();

        if (Program.DirectoriesToOpen.Count > 0)
        {
            services.AddSingleton(
                new TabsToOpenOnStart(
                    Program
                        .DirectoriesToOpen
                        .Select(d => new TabToOpen(null, new NativePath(d)))
                        .ToList()
                )
            );
        }

        DI.ServiceProvider = services.BuildServiceProvider();

        var logger = DI.ServiceProvider.GetRequiredService<ILogger<Application>>();
        logger.LogInformation("App initialization completed");
    }
    
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Log.Logger.Information("Creating MainWindow instance...");
            desktop.MainWindow = new MainWindow(InitializeApp)
            {
                DataContext = new MainWindowLoadingViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}