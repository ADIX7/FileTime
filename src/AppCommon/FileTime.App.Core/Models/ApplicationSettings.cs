using System.Reflection;

namespace FileTime.App.Core.Models;

public class ApplicationSettings : IApplicationSettings
{
    public string AppDataRoot { get; private set; } = null!;
    public string EnvironmentName { get; private set; } = null!;

    public string DataFolderName { get; } = "data";

    public ApplicationSettings()
    {
#if DEBUG
        InitDebugSettings();
#else
        InitReleaseSettings();
#endif
    }

    private void InitDebugSettings()
    {
        EnvironmentName = "Development";

        AppDataRoot = Path.Combine(Environment.CurrentDirectory, "appdata");
    }

    private void InitReleaseSettings()
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