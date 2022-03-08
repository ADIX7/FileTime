using System.Reflection;
using System;
using System.IO;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace FileTime.Avalonia
{
    public class Program
    {
        public static string AppDataRoot { get; }
        public static string EnvironmentName { get; }

        static Program()
        {

#if DEBUG
            EnvironmentName = "Development";

            AppDataRoot = Path.Combine(Environment.CurrentDirectory, "appdata");
#else
            EnvironmentName = "Release";

            var possibleDataRootsPaths = new List<string>()
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
                catch { }
            }

            if (appDataRoot == null) throw new UnauthorizedAccessException();
            AppDataRoot = appDataRoot;
#endif
        }

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
#if DEBUG
#else
            try
            {
#endif
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
#if DEBUG
#else
            }
            catch (Exception e)
            {
                var message = $"Ciritcal error cought in {nameof(Program)}";
                if (App.ServiceProvider?.GetService<ILogger<Program>>() is var logger && logger != null)
                {
                    logger.LogCritical(0, e, message);
                    return;
                }

                var logsPath = Path.Combine(AppDataRoot, "logs");
                var logsDirectory = new DirectoryInfo(logsPath);
                if (!logsDirectory.Exists) logsDirectory.Create();

                var logPath = Path.Combine(logsPath, "_criticalError.log");

                using var fileWriter = File.Open(logPath, FileMode.OpenOrCreate, FileAccess.Write);
                using var streamWriter = new StreamWriter(fileWriter);
                streamWriter.WriteLine(DateTime.Now.ToString() + ": " + message + "\n" + e.ToString() + "\n\n");
            }
#endif
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
