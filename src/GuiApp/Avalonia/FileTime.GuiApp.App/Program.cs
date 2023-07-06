using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.ReactiveUI;
using Serilog;
using Serilog.Debugging;

namespace FileTime.GuiApp.App;

public static class Program
{
    public static string AppDataRoot { get; private set; }
    public static string EnvironmentName { get; private set; }

    static Program()
    {
#if DEBUG
        InitDevelopment();
#else
        InitRelease();
#endif
        InitLogging();
        
        Log.Logger.Information("Early app starting...");
    }

    private static void InitDevelopment()
    {
        EnvironmentName = "Development";

        AppDataRoot = Path.Combine(Environment.CurrentDirectory, "appdata");
    }

    private static void InitRelease()
    {
        EnvironmentName = "Release";

        var possibleDataRootsPaths = new List<string>
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileTime"),
            Path.Combine(Assembly.GetEntryAssembly()?.Location ?? ".", "fallbackDataRoot")
        };

        string? appDataRoot = null;
        foreach (var possibleAppDataRoot in possibleDataRootsPaths)
        {
            try
            {
                var appDataRootDirectory = new DirectoryInfo(possibleAppDataRoot);
                if (!appDataRootDirectory.Exists) appDataRootDirectory.Create();

                //TODO write test
                appDataRoot = possibleAppDataRoot;
                break;
            }
            catch
            {
            }
        }

        AppDataRoot = appDataRoot ?? throw new UnauthorizedAccessException();
    }

    private static void InitLogging()
    {
        SelfLog.Enable(l => Debug.WriteLine(l));

        var logFolder = Path.Combine(AppDataRoot, "logs", "bootstrap");

        if (!Directory.Exists(logFolder)) Directory.CreateDirectory(logFolder);
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.FromLogContext()
            .WriteTo.File(
                Path.Combine(logFolder, "appLog.log"),
                fileSizeLimitBytes: 10 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                rollingInterval: RollingInterval.Day)
            .CreateBootstrapLogger();
    }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUI()
            .LogToTrace();
}