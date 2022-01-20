using FileTime.Core.Models;
using System.Linq;

namespace FileTime.Uno.IconProviders
{
    public class AwesomeIconProvider : IIconProvider
    {        
        public string GetImage(IItem item)
        {
            var icon = "solid/file.svg";
            if (item is IContainer)
            {
                icon = "solid/folder.svg";
            }
            else if (item is IElement element)
            {
                icon = !element.Name.Contains('.')
                    ? icon
                    : element.Name.Split('.').Last() switch
                    {
                        "pdf" => "solid/file-pdf",
                        "cs" => "solid/file-code",
                        _ => icon
                    };
            }
            return "ms-appx:///Assets/fontawesome/" + icon;
        }
    }
}