using System.Diagnostics;
using FileTime.App.Core;
using FileTime.App.Core.Configuration;
using FileTime.ConsoleUI;
using FileTime.ConsoleUI.App;
using FileTime.ConsoleUI.InfoProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Debugging;
using TerminalUI.ConsoleDrivers;

IConsoleDriver? driver = null;

(AppDataRoot, EnvironmentName) = Init.InitDevelopment();
InitLogging();
try
{
    var configuration = CreateConfiguration(args);

    var serviceProvider = DI.Initialize(configuration);

    if (HandleInfoProviders(args, serviceProvider)) return;

    driver = serviceProvider.GetRequiredService<IConsoleDriver>();
    Log.Logger.Debug("Using driver {Driver}", driver.GetType().Name);
    
    driver.SetCursorVisible(false);

    var app = serviceProvider.GetRequiredService<IApplication>();
    app.Run();
}
finally
{
    driver?.SetCursorVisible(true);
    driver?.Dispose();
}

static void InitLogging()
{
    SelfLog.Enable(l => Debug.WriteLine(l));

    var logFolder = Path.Combine(AppDataRoot, "logs", "bootstrap");

    if (!Directory.Exists(logFolder)) Directory.CreateDirectory(logFolder);

    Log.Logger = new LoggerConfiguration()
#if DEBUG || VERBOSE_LOGGING
        .MinimumLevel.Verbose()
#endif
        .Enrich.FromLogContext()
        .WriteTo.File(
            Path.Combine(logFolder, "appLog.log"),
            fileSizeLimitBytes: 10 * 1024 * 1024,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true)
        .CreateBootstrapLogger();
}

static IConfigurationRoot CreateConfiguration(string[] strings)
{
    var configurationRoot = new ConfigurationBuilder()
        .AddInMemoryCollection(MainConfiguration.Configuration)
        .AddInMemoryCollection(MainConsoleConfiguration.Configuration)
#if DEBUG
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
#endif
        .AddCommandLine(strings)
        .Build();
    return configurationRoot;
}

static bool HandleInfoProviders(string[] args, IServiceProvider serviceProvider)
{
    Dictionary<string, Action> infoProviders = new()
    {
        {
            "--info=colors",
            () => ColorSchema.PrintColorSchema(
                serviceProvider.GetRequiredService<ITheme>(),
                serviceProvider.GetRequiredService<IConsoleDriver>()
            )
        },
    };
    infoProviders.Add("--help", () => Help.PrintHelp(infoProviders.Keys));
    foreach (var infoProviderKey in infoProviders.Keys)
    {
        if (args.Contains(infoProviderKey))
        {
            infoProviders[infoProviderKey]();
            return true;
        }
    }


    return false;
}

public partial class Program
{
    public static string AppDataRoot { get; private set; }
    public static string EnvironmentName { get; private set; }
}