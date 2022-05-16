using FileTime.Core.Models;
using FileTime.GuiApp.Models;

namespace FileTime.GuiApp.IconProviders;

public interface IIconProvider
{
    ImagePath GetImage(IItem item);
    bool EnableAdvancedIcons { get; set; }
}