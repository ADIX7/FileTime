using FileTime.GuiApp.App.Models;

namespace FileTime.GuiApp.App.IconProviders;

public class SpecialPathWithIcon
{
    public string Path { get; }
    public ImagePath IconPath { get; }

    public SpecialPathWithIcon(string path, ImagePath iconPath)
    {
        Path = path.TrimEnd(System.IO.Path.DirectorySeparatorChar);
        IconPath = iconPath;
    }
}