using FileTime.ConsoleUI.App.Configuration.Theme;

namespace FileTime.ConsoleUI.App.Configuration;

public class StyleConfigurationRoot
{
    public string? Theme { get; set; }
    public Dictionary<string, ThemeConfiguration> Themes { get; set; } = new();
}