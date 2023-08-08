using FileTime.App.Core;
using FileTime.App.Core.Configuration;
using FileTime.ConsoleUI;
using FileTime.ConsoleUI.App;
using FileTime.ConsoleUI.Styles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TerminalUI.ConsoleDrivers;

IConsoleDriver driver = new WindowsDriver();
driver.Init();
ITheme theme;
if (driver.GetCursorPosition() is not {PosX: 0, PosY: 0})
{
    driver = new DotnetDriver();
    driver.Init();
    theme = DefaultThemes.ConsoleColorTheme;
}
else
{
    theme = DefaultThemes.Color256Theme;
}

driver.SetCursorVisible(false);

try
{
    (AppDataRoot, EnvironmentName) = Init.InitDevelopment();
    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(MainConfiguration.Configuration)
#if DEBUG
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
#endif
        .Build();

    var serviceCollection = new ServiceCollection();
    serviceCollection.TryAddSingleton<IConsoleDriver>(driver);
    serviceCollection.TryAddSingleton<ITheme>(theme);

    DI.Initialize(configuration, serviceCollection);

    var app = DI.ServiceProvider.GetRequiredService<IApplication>();
    app.Run();
}
finally
{
    driver.SetCursorVisible(true);
    driver.Dispose();
}

public partial class Program
{
    public static string AppDataRoot { get; private set; }
    public static string EnvironmentName { get; private set; }
}