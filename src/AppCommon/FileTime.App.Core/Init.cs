using System.Reflection;

namespace FileTime.App.Core;

public static class Init
{
    public static AppInitOptions InitDevelopment()
    {
        var environmentName = "Development";

        var appDataRoot = Path.Combine(Environment.CurrentDirectory, "appdata");

        return new(appDataRoot, environmentName);
    }

    public static AppInitOptions InitRelease()
    {
        var environmentName = "Release";

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

        return new(
            appDataRoot ?? throw new UnauthorizedAccessException(), 
            environmentName);
    }
}