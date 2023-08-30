using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.ReactiveUI;
using FileTime.App.Core;
using FileTime.GuiApp.App.InstanceManagement;
using FileTime.GuiApp.App.InstanceManagement.Messages;
using Serilog;
using Serilog.Debugging;
using Serilog.Extensions.Logging;

namespace FileTime.GuiApp;

public static class Program
{
    public static string AppDataRoot { get; private set; } = null!;
    public static string EnvironmentName { get; private set; } = null!;

    private static ILogger _logger = null!;

    internal static List<string> DirectoriesToOpen { get; } = new();

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
                outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true)
            .CreateBootstrapLogger();

        _logger = Log.ForContext(typeof(Program));
    }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static async Task Main(string[] args)
    {
#if DEBUG
        (AppDataRoot, EnvironmentName) = Init.InitDevelopment();
#endif
        if (AppDataRoot is null)
        {
            (AppDataRoot, EnvironmentName) = Init.InitRelease();
        }

        InitLogging();

        _logger.Information("Early app starting...");
        _logger.Information("Args ({ArgsLength}): {Args}", args.Length, $"\"{string.Join("\", \"", args)}\"");

        if (!await CheckDirectoryArguments(args))
        {
            NormalStartup(args);
        }
    }

    private static async Task<bool> CheckDirectoryArguments(string[] args)
    {
        var directoryArguments = new List<string>();
        foreach (var path in args)
        {
            var directory = new DirectoryInfo(path);
            if (directory.Exists)
            {
                directoryArguments.Add(directory.FullName);
            }
        }

        if (directoryArguments.Count == 0) return false;

        var loggerFactory = new SerilogLoggerFactory(Log.Logger);
        var logger = loggerFactory.CreateLogger(typeof(InstanceManager).Name);
        var instanceManager = new InstanceManager(new DummyInstanceMessageHandler(), logger);

        try
        {
            if (await instanceManager.TryConnectAsync())
            {
                await instanceManager.SendMessageAsync(new OpenContainers(directoryArguments));
                return true;
            }
        }
        catch
        {
            // ignored
        }

        DirectoriesToOpen.AddRange(directoryArguments);

        return false;
    }

    private static void NormalStartup(string[] args)
    {
        AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnTaskSchedulerUnobservedTaskException;
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
#if DEBUG
            .LogToTrace()
#endif
    ;

    private static void OnTaskSchedulerUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        => HandleUnhandledException(sender, e.Exception);

    private static void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        => HandleUnhandledException(sender, e.ExceptionObject as Exception);

    private static void OnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
        => HandleUnhandledException(sender, e.Exception);

    [Conditional("DEBUG")]
    private static void HandleUnhandledException(object? sender, Exception? ex, [CallerMemberName] string caller = "")
        => _logger.Debug(
            ex,
            "An unhandled exception come from '{Caller}' exception handler from an object of type '{Type}' and value '{Value}': {Exception}",
            caller,
            sender?.GetType().ToString() ?? "null",
            sender?.ToString() ?? "null",
            ex);
}