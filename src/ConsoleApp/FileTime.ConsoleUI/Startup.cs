using FileTime.ConsoleUI.App.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TerminalUI.Color;
using TerminalUI.ConsoleDrivers;
using TerminalUI.Styling;
using TerminalUI.Styling.Controls;
using IConsoleTheme = TerminalUI.Styling.ITheme;
using ConsoleTheme = TerminalUI.Styling.Theme;
using ITheme = FileTime.ConsoleUI.App.Styling.ITheme;
using Theme = FileTime.ConsoleUI.App.Styling.Theme;

namespace FileTime.ConsoleUI;

public static class Startup
{
    public static readonly Dictionary<string, Func<IConsoleDriver>> Drivers = new()
    {
        ["xterm"] = () => new XTermDriver(),
        ["dotnet"] = () => new DotnetDriver()
    };
    public static IServiceCollection AddConsoleDriver(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<IConsoleDriver>(sp =>
        {
            var appConfig = sp.GetRequiredService<IOptions<ConsoleApplicationConfiguration>>();

            IConsoleDriver? driver = null;
            if (appConfig.Value.ConsoleDriver is { } consoleDriver
                && Drivers.TryGetValue(consoleDriver, out var driverFactory))
            {
                driver = driverFactory();
                driver.Init();
            }

            if (driver == null)
            {
                driver = new XTermDriver();
                if (!driver.Init())
                {
                    driver = new DotnetDriver();
                    driver.Init();
                }
            }

            return driver;
        });

        return serviceCollection;
    }

    public static IServiceCollection AddTheme(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<ITheme>(sp =>
        {
            var colorProvider = sp.GetRequiredService<IColorProvider>();

            return new Theme(
                DefaultForegroundColor: null,
                DefaultForegroundAccentColor: colorProvider.RedForeground,
                DefaultBackgroundColor: null,
                ElementColor: colorProvider.DefaultForeground,
                ContainerColor: colorProvider.BlueForeground,
                MarkedItemForegroundColor: colorProvider.YellowForeground,
                MarkedItemBackgroundColor: null,
                MarkedSelectedItemForegroundColor: colorProvider.BlackForeground,
                MarkedSelectedItemBackgroundColor: colorProvider.YellowForeground,
                SelectedItemColor: colorProvider.BlackForeground,
                SelectedTabBackgroundColor: colorProvider.GreenBackground,
                WarningForegroundColor: colorProvider.YellowForeground,
                ErrorForegroundColor: colorProvider.RedForeground,
                ListViewItemTheme: new(
                    SelectedBackgroundColor: colorProvider.GrayBackground,
                    SelectedForegroundColor: colorProvider.BlackForeground
                ),
                ConsoleTheme: new ConsoleTheme
                {
                    ControlThemes = new ControlThemes
                    {
                        ProgressBar = new ProgressBarTheme
                        {
                            ForegroundColor = colorProvider.BlueForeground,
                            BackgroundColor = colorProvider.GrayBackground,
                            UnfilledForeground = colorProvider.GrayForeground,
                            UnfilledBackground = colorProvider.GrayBackground,
                        }
                    }
                }
            );
        });
        
        serviceCollection.TryAddSingleton<IColorProvider>(sp =>
        {
            var driver = sp.GetRequiredService<IConsoleDriver>();

            return driver switch
            {
                XTermDriver _ => new AnsiColorProvider(),
                DotnetDriver _ => new ConsoleColorProvider(),
                _ => throw new ArgumentOutOfRangeException(nameof(driver))
            };
            
        });

        return serviceCollection;
    }
}
