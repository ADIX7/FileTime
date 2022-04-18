using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FileTime.App.DependencyInjection;
using FileTime.GuiApp.ViewModels;
using FileTime.GuiApp.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileTime.GuiApp
{
    public partial class App : Application
    {
        static App()
        {
            DI.ServiceProvider ??= DependencyInjection
                .RegisterDefaultServices()
                .AddConfiguration()
                .RegisterLogging()
                .RegisterServices()
                .AddViewModels()
                .BuildServiceProvider()
                .InitSerilog();

            var logger = DI.ServiceProvider.GetRequiredService<ILogger<App>>();
            logger.LogInformation("App initialization completed");
        }
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

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
}