using System.Reflection;
using TerminalUI.ConsoleDrivers;

namespace FileTime.ConsoleUI.InfoProviders;

public static class Version
{
    public static void PrintVersionInfo(IConsoleDriver consoleDriver)
    {
        var version = Assembly.GetEntryAssembly()!.GetName().Version!;
        var versionString = $"{version.Major}.{version.Minor}.{version.Build}";
        consoleDriver.Write("FileTime version: " + versionString);
    }
}