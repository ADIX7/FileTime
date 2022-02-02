using FileTime.Avalonia.Models;
using FileTime.Core.Models;
using FileTime.Providers.Local;
using Syroot.Windows.IO;
using System.Collections.Generic;
using System.Linq;

namespace FileTime.Avalonia.IconProviders
{
    public class MaterialIconProvider : IIconProvider
    {
        private readonly List<SpecialPathWithIcon> _specialPaths = new();
        public bool EnableAdvancedIcons { get; set; } = true;

        public MaterialIconProvider()
        {
            _specialPaths.Add(new SpecialPathWithIcon(KnownFolders.Desktop.Path, GetAssetPath("desktop.svg")));
            _specialPaths.Add(new SpecialPathWithIcon(KnownFolders.Documents.Path, GetAssetPath("folder-resource.svg")));
            _specialPaths.Add(new SpecialPathWithIcon(KnownFolders.DownloadsLocalized.Path, GetAssetPath("folder-download.svg")));
            _specialPaths.Add(new SpecialPathWithIcon(KnownFolders.MusicLocalized.Path, GetAssetPath("folder-music.svg")));
            _specialPaths.Add(new SpecialPathWithIcon(KnownFolders.Pictures.Path, GetAssetPath("folder-images.svg")));
            _specialPaths.Add(new SpecialPathWithIcon(KnownFolders.Profile.Path, GetAssetPath("folder-home.svg")));
            _specialPaths.Add(new SpecialPathWithIcon(KnownFolders.Videos.Path, GetAssetPath("folder-video.svg")));
        }

        public ImagePath GetImage(IItem item)
        {
            var icon = item is IContainer ? "folder.svg" : "file.svg";
            string? localPath = item switch
            {
                LocalFolder folder => folder.Directory.FullName,
                LocalFile file => file.File.FullName,
                _ => null
            };

            if (EnableAdvancedIcons)
            {
                if (localPath != null && _specialPaths.Find(p => p.Path == localPath) is SpecialPathWithIcon specialPath)
                {
                    return specialPath.IconPath;
                }

                if (item is IElement element)
                {
                    if (element is LocalFile && (localPath?.EndsWith(".svg") ?? false))
                    {
                        return new ImagePath(ImagePathType.Absolute, localPath);
                    }
                    icon = !element.Name.Contains('.')
                        ? icon
                        : element.Name.Split('.').Last() switch
                        {
                            "cs" => "csharp.svg",
                            _ => icon
                        };
                }
            }
            return GetAssetPath(icon);
        }

        private static ImagePath GetAssetPath(string iconName)
        {
            return new ImagePath(ImagePathType.Asset, "/Assets/material/" + iconName);
        }
    }
}
