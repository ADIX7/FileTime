using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FileTime.App.Core;
using FileTime.Avalonia.ViewModels;
using FileTime.Avalonia.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace FileTime.Avalonia
{
    public class App : global::Avalonia.Application
    {
        public static IServiceProvider ServiceProvider { get; }

        static App()
        {
            ServiceProvider ??= DependencyInjection
                .RegisterDefaultServices()
                .AddConfiguration()
                .AddServices()
                .RegisterLogging()
                .AddViewModels()
                .BuildServiceProvider()
                .InitSerilog();

            var logger = ServiceProvider.GetService<ILogger<App>>();
            logger?.LogInformation("App initialization completed.");
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
                    DataContext = new MainPageLoadingViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}