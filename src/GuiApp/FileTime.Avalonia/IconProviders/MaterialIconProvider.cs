using Avalonia.Media.Imaging;
using FileTime.Avalonia.Misc;
using FileTime.Avalonia.Models;
using FileTime.Core.Models;
using FileTime.Providers.Local;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace FileTime.Avalonia.IconProviders
{
    public class MaterialIconProvider : IIconProvider
    {
        public bool EnableAdvancedIcons { get; set; } = true;

        public ImagePath GetImage(IItem item)
        {
            var icon = item is IContainer ? "folder.svg" : "file.svg";

            if (EnableAdvancedIcons)
            {
                if (item is IElement element)
                {
                    if (element is LocalFile localFile && (element.FullName?.EndsWith(".svg") ?? false))
                    {
                        return new ImagePath(ImagePathType.Absolute, localFile.File.FullName);
                    }
                    icon = !element.Name.Contains('.')
                        ? icon
                        : element.Name.Split('.').Last() switch
                        {
                            "cs" => "csharp.svg",
                            _ => icon
                        };
                }
                /*else if (item is LocalFolder folder && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var file = new FileInfo(Path.Combine(folder.FullName, "desktop.ini"));
                    if (file.Exists)
                    {
                        var lines = File.ReadAllLines(file.FullName);
                        if (Array.Find(lines, l => l.StartsWith("iconresource", StringComparison.OrdinalIgnoreCase)) is string iconLine)
                        {
                            var nameLineValue = string.Join('=', iconLine.Split('=')[1..]);
                            var environemntVariables = Environment.GetEnvironmentVariables();
                            foreach (var keyo in environemntVariables.Keys)
                            {
                                if (keyo is string key && environemntVariables[key] is string value)
                                {
                                    nameLineValue = nameLineValue.Replace($"%{key}%", value);
                                }
                            }

                            var parts = nameLineValue.Split(',');
                            if (parts.Length >= 2 && long.TryParse(parts[^1], out var parsedResourceId))
                            {
                                if (parsedResourceId < 0) parsedResourceId *= -1;

                                var extractedIcon = NativeMethodHelpers.GetIconResource(string.Join(',', parts[..^1]), (uint)parsedResourceId);

                                var extractedIconAsStream = new MemoryStream();
                                extractedIcon.Save(extractedIconAsStream);                                
                                extractedIconAsStream.Position = 0;

                                return new ImagePath(ImagePathType.Raw, new Bitmap(extractedIconAsStream));
                            }
                        }
                    }
                }*/
            }
            return new ImagePath(ImagePathType.Asset, "/Assets/material/" + icon);
        }
    }
}
