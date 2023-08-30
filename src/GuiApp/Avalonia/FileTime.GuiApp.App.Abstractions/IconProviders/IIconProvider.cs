using FileTime.Core.Models;
using FileTime.GuiApp.App.Models;

namespace FileTime.GuiApp.App.IconProviders;

public interface IIconProvider
{
    ImagePath GetImage(IItem item);
    ImagePath GetImage(string? localPath, bool isContainer, bool isLocalItem);
    bool EnableAdvancedIcons { get; set; }
}