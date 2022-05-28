using System.Drawing.Imaging;
using Avalonia.Media.Imaging;
using FileTime.Core.Models;
using FileTime.GuiApp.Helper;
using FileTime.GuiApp.Models;

namespace FileTime.GuiApp.IconProviders;

public static class WindowsSystemIconHelper
    {
        public static ImagePath? GetImageByDesktopIni(IContainer folder)
        {
            var file = new FileInfo(Path.Combine(folder.NativePath.Path, "desktop.ini"));
            if (file.Exists)
            {
                var lines = File.ReadAllLines(file.FullName);
                if (Array.Find(lines, l => l.StartsWith("iconresource", StringComparison.OrdinalIgnoreCase)) is string iconLine)
                {
                    var nameLineValue = string.Join('=', iconLine.Split('=')[1..]);
                    return GetImagePathByIconPath(nameLineValue);
                }
            }

            return null;
        }

        public static ImagePath GetImagePathByIconPath(string path)
        {
            var environemntVariables = Environment.GetEnvironmentVariables();
            foreach (var keyo in environemntVariables.Keys)
            {
                if (keyo is string key && environemntVariables[key] is string value)
                {
                    path = path.Replace($"%{key}%", value);
                }
            }

            var parts = path.Split(',');
            (var parsedResourceId, var path2) = parts.Length >= 2 && long.TryParse(parts[^1], out var id) 
                ? (id, NormalizePath(string.Join(',', parts[..^1]))) 
                : (0, NormalizePath(path));

            if (parsedResourceId == 0)
            {
                using var extractedIconAsStream = new MemoryStream();
                using var extractedIcon = System.Drawing.Icon.ExtractAssociatedIcon(path2).ToBitmap();
                extractedIcon.Save(extractedIconAsStream, ImageFormat.Png);
                extractedIconAsStream.Position = 0;
#pragma warning disable IDISP004 // Don't ignore created IDisposable
                return new ImagePath(ImagePathType.Raw, new Bitmap(extractedIconAsStream));
#pragma warning restore IDISP004 // Don't ignore created IDisposable
            }
            else
            {
                if (parsedResourceId < 0) parsedResourceId *= -1;

                using var extractedIcon = NativeMethodHelpers.GetIconResource(path2, (uint)parsedResourceId).ToBitmap();

                using var extractedIconAsStream = new MemoryStream();
                extractedIcon.Save(extractedIconAsStream, ImageFormat.Png);
                extractedIconAsStream.Position = 0;

#pragma warning disable IDISP004 // Don't ignore created IDisposable
                return new ImagePath(ImagePathType.Raw, new Bitmap(extractedIconAsStream));
#pragma warning restore IDISP004 // Don't ignore created IDisposable

            }
        }

        private static string NormalizePath(string path)
        {
            if (path.StartsWith('\"') && path.EndsWith('\"'))
            {
                return path[1..^1];
            }

            return path;
        }
    }