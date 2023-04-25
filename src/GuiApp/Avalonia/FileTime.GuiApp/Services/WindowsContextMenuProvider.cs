using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Media;
using FileTime.Core.Models;
using FileTime.GuiApp.Helper;
using FileTime.GuiApp.IconProviders;
using FileTime.Providers.Local;
using Microsoft.Win32;

namespace FileTime.GuiApp.Services;

public class WindowsContextMenuProvider : IContextMenuProvider
{
    public List<object> GetContextMenuForFolder(IContainer container)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) throw new NotSupportedException();

        var menuItems = new List<object>();

        if (container.Provider is ILocalContentProvider)
        {
            using var directoryKey = Registry.ClassesRoot.OpenSubKey("Directory");
            ProcessRegistryKey(directoryKey, menuItems, container!.NativePath!.Path);
        }

        return menuItems;
    }

    private void ProcessRegistryKey(RegistryKey? contextMenuContainer, List<object> menuItems, string folderPath)
    {
        using var shell = contextMenuContainer?.OpenSubKey("shell");
        if (shell == null) return;

        var shellSubKeys = shell.GetSubKeyNames();

        foreach (var shellKey in shellSubKeys.Select(k => shell.OpenSubKey(k)).OfType<RegistryKey>())
        {
            var textBase = shellKey.GetValue(null) as string ?? shellKey.GetValue("MUIVerb") as string;

            if (textBase == null) continue;

            string? text = null;
            if (textBase.StartsWith("@"))
            {
                var parts = textBase[1..].Split(',');
                if (parts.Length == 2 && long.TryParse(parts[1], out var parsedResourceId))
                {
                    if (parsedResourceId < 0) parsedResourceId *= -1;

                    text = NativeMethodHelpers.GetStringResource(string.Join(',', parts[..^1]), (uint) parsedResourceId);
                }
            }
            else
            {
                text = textBase;
            }

            if (text != null)
            {
                text = text.Replace("&", "");

                object? image = null;
                try
                {
                    if (shellKey.GetValue("Icon") is string iconPath)
                    {
                        var imagePath = WindowsSystemIconHelper.GetImagePathByIconPath(iconPath);
                        if (imagePath.Type == Models.ImagePathType.Raw)
                        {
                            image = new Image()
                            {
                                Source = (IImage) imagePath.Image!
                            };
                        }
                    }
                }
                catch
                {
                }

                using var commandKey = shellKey.OpenSubKey("command");
                if (shellKey.GetSubKeyNames().Contains("command") && commandKey?.GetValue(null) is string commandString)
                {
                    var item = new MenuItem() {Header = text, Icon = image};
                    item.Click += (o, e) => MenuItemClick(folderPath, commandString);
                    menuItems.Add(item);
                }
                else if (shellKey.GetValue("ExtendedSubCommandsKey") is string extendedCommands)
                {
                    var rootMenuItems = new List<object>();

                    ProcessRegistryKey(Registry.ClassesRoot.OpenSubKey(extendedCommands), rootMenuItems, folderPath);

                    var rootMenu = new MenuItem {Header = text, Icon = image};
                    foreach (var item in rootMenuItems)
                    {
                        rootMenu.Items.Add(item);
                    }

                    menuItems.Add(rootMenu);
                }
            }
        }
    }

    private static void MenuItemClick(string folderPath, string commandString)
    {
        var commandPartsWithoutAp = commandString.Split('\"').ToList();
        var commandParts = new List<List<string>>();

        for (var i = 0; i < commandPartsWithoutAp.Count; i++)
        {
            if (i % 2 == 0)
            {
                commandParts.Add(commandPartsWithoutAp[i].Split(' ').ToList());
            }
            else
            {
                commandParts.Add(new List<string> {commandPartsWithoutAp[i]});
            }
        }

        for (var i = 0; i < commandParts.Count; i++)
        {
            for (var i2 = 0; i2 < commandParts[i].Count; i2++)
            {
                commandParts[i][i2] = commandParts[i][i2].Replace("%1", folderPath).Replace("%V", folderPath);
            }
        }

        var commandPartsWithoutEmpty = commandParts.SelectMany(c => c).Where(c => !string.IsNullOrWhiteSpace(c)).ToList();

        if (commandPartsWithoutEmpty.Count == 1)
        {
            Process.Start(commandPartsWithoutEmpty[0]);
        }
        else if (commandPartsWithoutEmpty.Count > 1)
        {
            var paramStartIndex1 = -1;
            var paramStartIndex2 = -1;
            var found = false;

            for (var x = 0; x < commandParts.Count && paramStartIndex1 == -1; x++)
            {
                for (var y = 0; y < commandParts[x].Count; y++)
                {
                    if (found)
                    {
                        paramStartIndex1 = x;
                        paramStartIndex2 = y;
                        break;
                    }

                    if (commandParts[x][y] == commandPartsWithoutEmpty[0])
                    {
                        found = true;
                    }
                }
            }

            var arguments = SumList(commandParts, paramStartIndex1, paramStartIndex2);

            try
            {
                using var process = new Process();
                process.StartInfo.FileName = commandPartsWithoutEmpty[0];
                process.StartInfo.Arguments = arguments;
                process.Start();
            }
            catch
            {
                if (commandParts[0].Count > 0)
                {
                    var executable = "";
                    var lastExecutablePart = 0;
                    for (lastExecutablePart = 0; !File.Exists(executable) && lastExecutablePart < commandParts[0].Count; lastExecutablePart++)
                    {
                        executable += (lastExecutablePart == 0 ? "" : " ") + commandParts[0][lastExecutablePart];
                    }

                    lastExecutablePart--;

                    if (File.Exists(executable))
                    {
                        try
                        {
                            var (paramStartX, paramStartY) = GetCoordinatesFrom(commandParts, 1, 0, lastExecutablePart);
                            arguments = SumList(commandParts, paramStartX, paramStartY);

                            using var process = new Process();
                            process.StartInfo.FileName = executable;
                            process.StartInfo.Arguments = arguments;
                            process.Start();
                        }
                        catch
                        {
                            //TODO: error message
                        }
                    }
                }
                //TODO: ELSE error message
            }
        }
    }

    private static string SumList(List<List<string>> data, int paramStartIndex1, int paramStartIndex2)
    {
        var result = "";

        for (var x = paramStartIndex1; x < data.Count; x++)
        {
            if (x % 2 == 1) result += "\"";

            result += string.Join(
                ' ',
                x == paramStartIndex1
                    ? data[x].Skip(paramStartIndex2)
                    : data[x]
            );

            if (x % 2 == 1) result += "\"";
        }

        return result;
    }

    private static (int, int) GetCoordinatesFrom(List<List<string>> data, int skip, int startX = 0, int startY = 0)
    {
        int skipping = 0;
        var x = startX;
        var y = startY;

        var resultX = -1;
        var resultY = -1;
        for (; x < data.Count && resultX == -1; x++, y = 0)
        {
            for (; y < data[x].Count; y++)
            {
                if (skipping == skip)
                {
                    resultX = x;
                    resultY = y;
                    break;
                }

                skipping++;
            }
        }

        return (resultX, resultY);
    }
}