using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia.Controls;
using Avalonia.Media;
using FileTime.Core.Models;
using FileTime.GuiApp.Helper;
using FileTime.GuiApp.IconProviders;
using FileTime.Providers.Local;
using Microsoft.Win32;

namespace FileTime.GuiApp.Services;

[SupportedOSPlatform("windows")]
public class WindowsContextMenuProvider : IContextMenuProvider
{
    public List<object> GetContextMenuForFolder(IContainer container)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) throw new NotSupportedException();

        var menuItems = new List<object>();
        if (container.Provider is not ILocalContentProvider) return menuItems;

        using var directoryKey = Registry.ClassesRoot.OpenSubKey("Directory");
        ProcessRegistryKeyForContainer(directoryKey, menuItems, container!.NativePath!.Path);

        return menuItems;
    }

    private void ProcessRegistryKeyForContainer(RegistryKey? contextMenuContainer, List<object> menuItems, string folderPath)
    {
        using var shell = contextMenuContainer?.OpenSubKey("shell");
        if (shell == null) return;

        var shellSubKeys = shell.GetSubKeyNames();

        foreach (var shellKey in shellSubKeys.Select(k => shell.OpenSubKey(k)).OfType<RegistryKey>())
        {
            var displayTextBase =
                shellKey.GetValue(null) as string
                ?? shellKey.GetValue("MUIVerb") as string
                ?? shellKey.Name.Split('\\').Last();

            string? displayText = null;
            displayText = displayTextBase.StartsWith("@")
                ? ResolveText(displayTextBase)
                : displayTextBase;

            if (displayText is null) continue;

            displayText = displayText.Replace("&", "");

            var image = shellKey.GetValue("Icon") is string iconPath
                ? ResolveImage(iconPath)
                : null;

            using var commandKey = shellKey.OpenSubKey("command");
            if (commandKey?.GetValueNames().Contains("DelegateExecute") ?? false) continue;

            if (GetCommandKey(shellKey, commandKey) is { } commandString)
            {
                var item = new MenuItem {Header = displayText, Icon = image};
                item.Click += (o, e) => HandleStartCommandMenuItemClick(folderPath, commandString);
                menuItems.Add(item);
            }
            else if (shellKey.GetValue("ExtendedSubCommandsKey") is string extendedCommands)
            {
                var rootMenuItems = new List<object>();

                ProcessRegistryKeyForContainer(Registry.ClassesRoot.OpenSubKey(extendedCommands), rootMenuItems, folderPath);

                if (rootMenuItems.Count == 0) continue;

                var rootMenu = new MenuItem {Header = displayText, Icon = image};
                foreach (var item in rootMenuItems)
                {
                    rootMenu.Items.Add(item);
                }

                menuItems.Add(rootMenu);
            }
        }

        static string? ResolveText(string textBase)
        {
            var parts = textBase[1..].Split(',');
            if (parts.Length == 2 && long.TryParse(parts[1], out var parsedResourceId))
            {
                if (parsedResourceId < 0) parsedResourceId *= -1;

                return NativeMethodHelpers.GetStringResource(string.Join(',', parts[..^1]), (uint) parsedResourceId);
            }

            return null;
        }

        static string? GetCommandKey(RegistryKey shellKey, RegistryKey? commandKey)
        {
            return
                shellKey.GetSubKeyNames().Contains("command")
                && commandKey?.GetValue(null) is string commandString
                    ? commandString
                    : null;
        }
    }

    public List<object> GetContextMenuForFile(IElement element)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) throw new NotSupportedException();

        var menuItems = new List<object>();

        if (element.Provider is not ILocalContentProvider) return menuItems;

        var extension = element.Name.Split('.').LastOrDefault();
        if (extension is null) return menuItems;

        using var extensionKey = Registry.ClassesRoot.OpenSubKey("." + extension);
        ProcessRegistryKeyForElement(extensionKey, menuItems, element!.NativePath!.Path);

        return menuItems;
    }

    private void ProcessRegistryKeyForElement(RegistryKey? extensionKey, List<object> menuItems, string path)
    {
        var openWithItems = GetElementOpenWithItems(extensionKey, path);
        if (openWithItems.Count > 0)
        {
            var openWithMenuItem = new MenuItem {Header = "Open with"};
            foreach (var openWithItem in openWithItems)
            {
                openWithMenuItem.Items.Add(openWithItem);
            }

            menuItems.Add(openWithMenuItem);
        }
    }

    private List<object> GetElementOpenWithItems(RegistryKey? extensionKey, string path)
    {
        List<object> menuItems = new();
        var openWithProgIds = extensionKey?.OpenSubKey("OpenWithProgids");
        if (openWithProgIds is null) return menuItems;

        foreach (var valueName in openWithProgIds.GetValueNames())
        {
            var programRegistryKey = Registry.ClassesRoot.OpenSubKey(valueName);
            if (programRegistryKey is null) continue;

            var programOpenKey = programRegistryKey.OpenSubKey("shell")?.OpenSubKey("open");
            if (programOpenKey is null) continue;


            //Try get display name
            var displayText = GetDisplayText(programRegistryKey, programOpenKey);

            //Try get executable path
            var programCommandKey = programOpenKey.OpenSubKey("command");
            if (programCommandKey?.GetValue(null) is not string command) continue;

            var commandParts = ChopCommand(command);

            var (executable, _) = TryGetExecutablePath(commandParts);

            if (executable is null) continue;

            displayText ??= Registry.ClassesRoot
                .OpenSubKey("Local Settings")
                ?.OpenSubKey("Software")
                ?.OpenSubKey("Microsoft")
                ?.OpenSubKey("Windows")
                ?.OpenSubKey("Shell")
                ?.OpenSubKey("MuiCache")
                ?.GetValue(executable + ".FriendlyAppName") as string;

            if (displayText is null) continue;

            var menuItem = new MenuItem {Header = displayText, Icon = ResolveImage(executable)};
            menuItem.Click +=
                (_, _) => HandleStartCommandMenuItemClick(
                    path,
                    command,
                    commandParts: commandParts);
            menuItems.Add(menuItem);
        }

        return menuItems;

        static string? GetDisplayText(RegistryKey programRegistryKey, RegistryKey programOpenKey)
        {
            if (programRegistryKey.GetValue("FriendlyAppName") is string rootFriendAppName)
                return rootFriendAppName;

            if (programOpenKey.GetValue("FriendlyAppName") is string openFriendAppName) return openFriendAppName;

            return null;
        }
    }

    private static void HandleStartCommandMenuItemClick(
        string placeholderValue,
        string commandString,
        List<List<string>>? commandParts = null
    )
    {
        commandParts ??= ChopCommand(commandString);

        ReplacePlaceholders(commandParts, placeholderValue);

        var commandPartsWithoutEmpty = commandParts
            .SelectMany(c => c)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToList();

        if (commandPartsWithoutEmpty.Count == 0)
            return;

        if (commandPartsWithoutEmpty.Count == 1)
        {
            Process.Start(commandPartsWithoutEmpty[0]);
            return;
        }


        if (commandParts[0].Count > 0)
        {
            var (executable, lastExecutablePart) = TryGetExecutablePath(commandParts);

            if (executable is not null)
            {
                try
                {
                    var (paramStartX, paramStartY) = GetPositionInArrayFromBySkipping(commandParts, 1, 0, lastExecutablePart);
                    var arguments = SumList(commandParts, paramStartX, paramStartY);

                    using var process = new Process();
                    process.StartInfo.FileName = executable;
                    process.StartInfo.Arguments = arguments.TrimStart();
                    process.Start();

                    return;
                }
                catch
                {
                    //TODO: error message
                }
            }
        }

        var (argumentsStartIndexX, argumentsStartIndexY) = FindArgumentStartPosition(commandParts, commandPartsWithoutEmpty);
        var arguments2 = SumList(commandParts, argumentsStartIndexX, argumentsStartIndexY);
        using var process2 = new Process();
        process2.StartInfo.FileName = commandPartsWithoutEmpty[0];
        process2.StartInfo.Arguments = arguments2;
        process2.Start();

        static void ReplacePlaceholders(List<List<string>> commandParts, string placeholderValue)
        {
            foreach (var commandPart in commandParts)
            {
                for (var i2 = 0; i2 < commandPart.Count; i2++)
                {
                    commandPart[i2] = commandPart[i2]
                        .Replace("%1", placeholderValue)
                        .Replace("%V", placeholderValue);
                }
            }
        }

        static (int argumentsStartIndexX, int argumentsStartIndexY) FindArgumentStartPosition(
            List<List<string>> commandParts,
            List<string> commandPartsWithoutEmpty)
        {
            var argumentsStartIndexX = -1;
            var argumentsStartIndexY = -1;
            var found = false;

            for (var x = 0; x < commandParts.Count && argumentsStartIndexX == -1; x++)
            {
                for (var y = 0; y < commandParts[x].Count; y++)
                {
                    if (found)
                    {
                        argumentsStartIndexX = x;
                        argumentsStartIndexY = y;
                        break;
                    }

                    if (commandParts[x][y] == commandPartsWithoutEmpty[0])
                    {
                        found = true;
                    }
                }
            }

            return (argumentsStartIndexX, argumentsStartIndexY);
        }

        static (int positionX, int positionY) GetPositionInArrayFromBySkipping(
            List<List<string>> data,
            int skip,
            int startX = 0,
            int startY = 0
        )
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

    private static List<List<string>> ChopCommand(string commandString)
    {
        var commandPartsWithoutQuotationMark = commandString.Split('\"').ToList();
        var commandParts = new List<List<string>>();

        for (var i = 0; i < commandPartsWithoutQuotationMark.Count; i++)
        {
            if (i % 2 == 0)
            {
                commandParts.Add(commandPartsWithoutQuotationMark[i].Split(' ').ToList());
            }
            else
            {
                commandParts.Add(new List<string> {commandPartsWithoutQuotationMark[i]});
            }
        }

        return commandParts;
    }

    static (string? executable, int lastExecutablePart) TryGetExecutablePath(List<List<string>> commandParts)
    {
        var executable = "";
        var lastExecutablePart = 0;

        //Note: If the first block is empty, we will use the second one
        //Note: This can happen (or rather, the common case) when the command starts with ", for example 
        //          `"c:\...\...\xyz.exe" someParam` (without the ``)
        var executableIndex = commandParts[0].Count == 0 || commandParts[0].All(string.IsNullOrWhiteSpace)
            ? 1
            : 0;

        for (; !File.Exists(executable) && lastExecutablePart < commandParts[executableIndex].Count; lastExecutablePart++)
        {
            executable += (lastExecutablePart == 0 ? "" : " ") + commandParts[executableIndex][lastExecutablePart];
        }

        if (executableIndex == 1) lastExecutablePart += commandParts[0].Count;

        lastExecutablePart--;

        return (File.Exists(executable) ? executable : null, lastExecutablePart);
    }

    private static Image? ResolveImage(string iconPath)
    {
        try
        {
            var imagePath = WindowsSystemIconHelper.GetImagePathByIconPath(iconPath);
            if (imagePath.Type != Models.ImagePathType.Raw) return null;

            return new Image
            {
                Source = (IImage) imagePath.Image!
            };
        }
        catch
        {
        }

        return null;
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
}