using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FileTime.App.CommandPalette;
using FileTime.App.DependencyInjection;
using FileTime.App.FrequencyNavigation;
using FileTime.App.Search;
using FileTime.GuiApp.Font;
using FileTime.GuiApp.ViewModels;
using FileTime.GuiApp.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileTime.GuiApp.App;

public class App : Application
{
    static App()
    {
        var configuration = Startup.CreateConfiguration();
        DI.ServiceProvider = DependencyInjection
            .RegisterDefaultServices()
            .AddFrequencyNavigation()
            .AddCommandPalette()
            .AddSearch()
            .AddConfiguration(configuration)
            .ConfigureFont(configuration)
            .RegisterLogging()
            .RegisterServices()
            .AddViewModels()
            .BuildServiceProvider();

        var logger = DI.ServiceProvider.GetRequiredService<ILogger<App>>();
        logger.LogInformation("App initialization completed");
    }
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowLoadingViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}