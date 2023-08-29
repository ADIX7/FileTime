using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace FileTime.GuiApp.App.Settings;

public class DefaultBrowserRegister : IDefaultBrowserRegister
{
    private const string WinEKeyPathSub = @"shell\opennewwindow\command";
    private const string WinEKeyPathRoot = @"SOFTWARE\Classes\CLSID\{52205fd8-5dfb-447d-801a-d0b52f2e83e1}";
    private const string WinEKeyPath = $@"{WinEKeyPathRoot}\{WinEKeyPathSub}";
    private const string FullWinEKeyPath = $@"HKEY_CURRENT_USER\{WinEKeyPath}";
    private const string FullWinEKeyPathRoot = $@"HKEY_CURRENT_USER\{WinEKeyPathRoot}";

    private const string DefaultBrowserKeyPath = @"SOFTWARE\Classes\Folder\shell\open\command";
    private const string FullDefaultBrowserKeyPath = $@"HKEY_CURRENT_USER\{DefaultBrowserKeyPath}";

    private readonly ILogger<DefaultBrowserRegister> _logger;

    public DefaultBrowserRegister(ILogger<DefaultBrowserRegister> logger)
    {
        _logger = logger;
    }

    public async void RegisterAsDefaultEditor()
    {
        string? tempFile = null;
        try
        {
            tempFile = Path.GetTempFileName() + ".reg";
            var hexPath = GetFileTimeHexPath("\"%1\"");
            await using (var streamWriter = new StreamWriter(tempFile))
            {
                await streamWriter.WriteLineAsync("Windows Registry Editor Version 5.00");
                await streamWriter.WriteLineAsync();
                var s = $$"""
                          [{{FullDefaultBrowserKeyPath}}]
                              
                          @={{hexPath}}
                          "DelegateExecute"=""

                          [HKEY_CURRENT_USER\SOFTWARE\Classes\Folder\shell\explore\command]
                              
                          @={{hexPath}}
                          "DelegateExecute"=""
                          """;
                await streamWriter.WriteLineAsync(s);

                await streamWriter.FlushAsync();
            }

            await StartRegeditProcessAsync(tempFile);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while registering Win+E shortcut");
        }
        finally
        {
            if (tempFile is not null)
            {
                File.Delete(tempFile);
            }
        }
    }

    public async void UnregisterAsDefaultEditor()
    {
        string? tempFile = null;
        try
        {
            tempFile = Path.GetTempFileName() + ".reg";
            await using (var streamWriter = new StreamWriter(tempFile))
            {
                await streamWriter.WriteLineAsync("Windows Registry Editor Version 5.00");
                await streamWriter.WriteLineAsync();
                var s = $$"""
                          [HKEY_CURRENT_USER\SOFTWARE\Classes\Folder\shell\open]
                          "MultiSelectModel"="Document"

                          [{{FullDefaultBrowserKeyPath}}]
                          @=hex(2):25,00,53,00,79,00,73,00,74,00,65,00,6d,00,52,00,6f,00,6f,00,74,00,25,\
                          00,5c,00,45,00,78,00,70,00,6c,00,6f,00,72,00,65,00,72,00,2e,00,65,00,78,00,\
                          65,00,00,00
                          "DelegateExecute"="{11dbb47c-a525-400b-9e80-a54615a090c0}"

                          [HKEY_CURRENT_USER\SOFTWARE\Classes\Folder\shell\explore]
                          "LaunchExplorerFlags"=dword:00000018
                          "MultiSelectModel"="Document"
                          "ProgrammaticAccessOnly"=""

                          [HKEY_CURRENT_USER\SOFTWARE\Classes\Folder\shell\explore\command]
                          @=-
                          "DelegateExecute"="{11dbb47c-a525-400b-9e80-a54615a090c0}"
                          """;
                await streamWriter.WriteLineAsync(s);

                await streamWriter.FlushAsync();
            }

            await StartRegeditProcessAsync(tempFile);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while registering Win+E shortcut");
        }
        finally
        {
            if (tempFile is not null)
            {
                File.Delete(tempFile);
            }
        }
    }

    public async void RegisterWinEShortcut()
    {
        string? tempFile = null;
        try
        {
            tempFile = Path.GetTempFileName() + ".reg";
            var hexPath = GetFileTimeHexPath();
            await using (var streamWriter = new StreamWriter(tempFile))
            {
                await streamWriter.WriteLineAsync("Windows Registry Editor Version 5.00");
                await streamWriter.WriteLineAsync();
                var s = $$"""
                          [{{FullWinEKeyPath}}]
                              
                          @={{hexPath}}
                          "DelegateExecute"=""
                          """;
                await streamWriter.WriteLineAsync(s);

                await streamWriter.FlushAsync();
            }

            await StartRegeditProcessAsync(tempFile);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while registering Win+E shortcut");
        }
        finally
        {
            if (tempFile is not null)
            {
                File.Delete(tempFile);
            }
        }
    }

    public async void UnregisterWinEShortcut()
    {
        string? tempFile = null;
        try
        {
            tempFile = Path.GetTempFileName() + ".reg";
            await using (var streamWriter = new StreamWriter(tempFile))
            {
                await streamWriter.WriteLineAsync("Windows Registry Editor Version 5.00");
                await streamWriter.WriteLineAsync();
                var s = $"[-{FullWinEKeyPathRoot}]";
                await streamWriter.WriteLineAsync(s);

                await streamWriter.FlushAsync();
            }

            await StartRegeditProcessAsync(tempFile);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while unregistering Win+E shortcut");
        }
        finally
        {
            if (tempFile is not null)
            {
                File.Delete(tempFile);
            }
        }
    }

    public bool IsWinEShortcut() => IsKeyContainsPath(WinEKeyPath);

    public bool IsDefaultFileBrowser() => IsKeyContainsPath(DefaultBrowserKeyPath);

    private bool IsKeyContainsPath(string key)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return false;

        using var subKey = Registry.CurrentUser.OpenSubKey(key);
        var command = (string?) subKey?.GetValue(string.Empty);

        return !string.IsNullOrEmpty(command) && command.Contains(GetFileTimeExecutablePath());
    }

    private string GetFileTimeHexPath(string? parameter = null)
    {
        var fileTimePath = GetFileTimeExecutablePath();
        var hexPath = GetRegistryHexString("\"" + fileTimePath + "\"" + (parameter is null ? "" : " " + parameter));
        return hexPath;
    }

    private async Task StartRegeditProcessAsync(string regFilePath)
    {
        try
        {
            using var regProcess = Process.Start(
                new ProcessStartInfo(
                    "regedit.exe",
                    @$"/s ""{regFilePath}""")
                {
                    UseShellExecute = true,
                    Verb = "runas"
                });

            if (regProcess is not null)
            {
                await regProcess.WaitForExitAsync();
            }
        }
        catch
        {
            // Canceled UAC
        }
    }

    private string GetFileTimeExecutablePath()
        => Process.GetCurrentProcess().MainModule!.FileName;

    private string GetRegistryHexString(string s)
    {
        var bytes = Encoding.Unicode.GetBytes(s);
        return "hex(2):" + BitConverter.ToString(bytes).Replace("-", ",");
    }
}