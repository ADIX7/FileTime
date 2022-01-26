using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FileTime.App.Core;
using FileTime.Avalonia.ViewModels;
using FileTime.Avalonia.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FileTime.Avalonia
{
    public class App : global::Avalonia.Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        public App()
        {
            ServiceProvider ??= DependencyInjection
                .RegisterDefaultServices()
                .AddViewModels()
                .AddServices()
                .BuildServiceProvider();
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
                    ViewModel = ServiceProvider.GetService<MainPageViewModel>(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}