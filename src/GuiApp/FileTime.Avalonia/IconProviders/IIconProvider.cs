using FileTime.Avalonia.Models;
using FileTime.Core.Models;

namespace FileTime.Avalonia.IconProviders
{
    public interface IIconProvider
    {
        ImagePath GetImage(IItem item);
        bool EnableAdvancedIcons { get; set; }
    }
}