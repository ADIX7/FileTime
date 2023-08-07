using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.ReactiveUI;
using FileTime.App.Core;
using Serilog;
using Serilog.Debugging;

namespace FileTime.GuiApp;

public static class Program
{
    public static string AppDataRoot { get; private set; }
    public static string EnvironmentName { get; private set; }

    private static void InitLogging()
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
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true)
            .CreateBootstrapLogger();
    }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
#if DEBUG
        (AppDataRoot, EnvironmentName) = Init.InitDevelopment();
#endif
        if (AppDataRoot is null)
        {
            (AppDataRoot, EnvironmentName) = Init.InitRelease();
        }

        InitLogging();

        Log.Logger.Information("Early app starting...");

        AppDomain.CurrentDomain.FirstChanceException -= OnFirstChanceException;
        AppDomain.CurrentDomain.UnhandledException -= OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnTaskSchedulerUnobservedTaskException;
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
        => AppBuilder.Configure<Application>()
            .UsePlatformDetect()
            .UseReactiveUI()
            .LogToTrace();

    private static void OnTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        => HandleUnhandledException(sender, e.Exception);

    private static void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        => HandleUnhandledException(sender, e.ExceptionObject as Exception);

    private static void OnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
        => HandleUnhandledException(sender, e.Exception);

    private static void HandleUnhandledException(object? sender, Exception? ex, [CallerMemberName] string caller = "")
    {
        Log.Fatal(
            ex,
            "An unhandled exception come from '{Caller}' exception handler from an object of type '{Type}' and value '{Value}': {Exception}",
            caller,
            sender?.GetType().ToString() ?? "null",
            sender?.ToString() ?? "null",
            ex);

        Log.CloseAndFlush();
    }
}