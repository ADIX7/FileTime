using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FileTime.App.CommandPalette;
using FileTime.App.ContainerSizeScanner;
using FileTime.App.DependencyInjection;
using FileTime.App.FrequencyNavigation;
using FileTime.App.Search;
using FileTime.GuiApp.App;
using FileTime.GuiApp.App.Font;
using FileTime.GuiApp.App.ViewModels;
using FileTime.GuiApp.App.Views;
using FileTime.Server;
using FileTime.Tools.Compression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileTime.GuiApp;

public class Application : Avalonia.Application
{
    private static void InitializeApp()
    {
        var configuration = Startup.CreateConfiguration();
        DI.ServiceProvider = DependencyInjection
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
            .AddViewModels()
            .BuildServiceProvider();

        var logger = DI.ServiceProvider.GetRequiredService<ILogger<Application>>();
        logger.LogInformation("App initialization completed");
    }
    
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(InitializeApp)
            {
                DataContext = new MainWindowLoadingViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}