using FileTime.GuiApp.Models;

namespace FileTime.GuiApp.IconProviders;

public class SpecialPathWithIcon
{
    public string Path { get; }
    public ImagePath IconPath { get; }

    public SpecialPathWithIcon(string path, ImagePath iconPath)
    {
        Path = path;
        IconPath = iconPath;
    }
}