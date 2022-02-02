using System.IO;
using Avalonia.Media.Imaging;
using FileTime.Avalonia.Misc;
using FileTime.Avalonia.Models;
using FileTime.Core.Models;
using FileTime.Providers.Local;

namespace FileTime.Avalonia.IconProviders
{
    public class SystemIconProvider : IIconProvider
    {
        public bool EnableAdvancedIcons { get; set; }

        public ImagePath GetImage(IItem item)
        {
            if (item is LocalFile file)
            {
                var extractedIconAsStream = new MemoryStream();
                var extractedIcon = System.Drawing.Icon.ExtractAssociatedIcon(file.File.FullName);
                extractedIcon.Save(extractedIconAsStream);
                extractedIconAsStream.Position = 0;
                return new ImagePath(ImagePathType.Raw, new Bitmap(extractedIconAsStream));
            }

            var icon = item is IContainer ? "folder.svg" : "file.svg";
            return new ImagePath(ImagePathType.Asset, "/Assets/material/" + icon);
        }
    }
}