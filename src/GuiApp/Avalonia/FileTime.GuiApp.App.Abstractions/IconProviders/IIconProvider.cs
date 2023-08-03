using FileTime.Core.Models;
using FileTime.GuiApp.App.Models;

namespace FileTime.GuiApp.App.IconProviders;

public interface IIconProvider
{
    ImagePath GetImage(IItem item);
    bool EnableAdvancedIcons { get; set; }
}