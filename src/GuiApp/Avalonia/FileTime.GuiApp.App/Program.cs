using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.ReactiveUI;

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

        void InitDevelopment()
        {
            EnvironmentName = "Development";

            AppDataRoot = Path.Combine(Environment.CurrentDirectory, "appdata");
        }

        void InitRelease()
        {
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
                catch
                {
                }
            }

            AppDataRoot = appDataRoot ?? throw new UnauthorizedAccessException();
        }
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