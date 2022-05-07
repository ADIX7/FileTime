using System;
using System.IO;
using Avalonia;
using Avalonia.ReactiveUI;

namespace FileTime.GuiApp;

public static class Program
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
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseReactiveUI()
            .LogToTrace();
}