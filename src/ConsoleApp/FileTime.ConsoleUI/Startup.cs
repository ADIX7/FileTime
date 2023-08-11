using FileTime.ConsoleUI.App;
using FileTime.ConsoleUI.App.Configuration;
using FileTime.ConsoleUI.App.Styling;
using FileTime.ConsoleUI.Styles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TerminalUI.ConsoleDrivers;

namespace FileTime.ConsoleUI;

public static class Startup
{
    public static readonly Dictionary<string, Func<IConsoleDriver>> Drivers = new()
    {
        ["windows"] = () => new XTermDriver(),
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
            var driver = sp.GetRequiredService<IConsoleDriver>();

            return driver switch
            {
                XTermDriver _ => DefaultThemes.Color256Theme,
                DotnetDriver _ => DefaultThemes.ConsoleColorTheme,
                _ => throw new ArgumentOutOfRangeException(nameof(driver))
            };
        });

        return serviceCollection;
    }
}