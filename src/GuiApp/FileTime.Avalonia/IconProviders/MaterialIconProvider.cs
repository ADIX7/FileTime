using FileTime.Core.Models;
using FileTime.Providers.Local;
using System.Linq;

namespace FileTime.Avalonia.IconProviders
{
    public class MaterialIconProvider : IIconProvider
    {
        public string GetImage(IItem item)
        {
            var icon = "file.svg";
            if (item is IContainer)
            {
                icon = "folder.svg";
            }
            else if (item is IElement element)
            {
                if(element is LocalFile localFile && element.FullName.EndsWith(".svg"))
                {
                    return localFile.File.FullName;
                }
                icon = !element.Name.Contains('.')
                    ? icon
                    : element.Name.Split('.').Last() switch
                    {
                        "cs" => "csharp.svg",
                        _ => icon
                    };
            }
            return "/Assets/material/" + icon;
        }
    }
}
