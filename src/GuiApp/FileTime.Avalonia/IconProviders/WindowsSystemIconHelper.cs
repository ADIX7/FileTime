using System;
using System.IO;
using Avalonia.Media.Imaging;
using FileTime.Avalonia.Misc;
using FileTime.Avalonia.Models;
using FileTime.Providers.Local;

namespace FileTime.Avalonia.IconProviders
{
    public static class WindowsSystemIconHelper
    {
        public static ImagePath? GetImageByDesktopIni(LocalFolder folder)
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

            return null;
        }
    }
}