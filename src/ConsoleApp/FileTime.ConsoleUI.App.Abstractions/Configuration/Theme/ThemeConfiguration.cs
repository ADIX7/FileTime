namespace FileTime.ConsoleUI.App.Configuration.Theme;

public class ThemeConfiguration
{
    public string? DefaultForegroundColor { get; set; }
    public string? DefaultForegroundAccentColor { get; set; }
    public string? DefaultBackgroundColor { get; set; }
    public string? ElementColor { get; set; }
    public string? ContainerColor { get; set; }
    public string? MarkedItemForegroundColor { get; set; }
    public string? MarkedItemBackgroundColor { get; set; }
    public string? MarkedSelectedItemForegroundColor { get; set; }
    public string? MarkedSelectedItemBackgroundColor { get; set; }
    public string? SelectedItemColor { get; set; }
    public string? SelectedTabBackgroundColor { get; set; }
    public string? WarningForegroundColor { get; set; }
    public string? ErrorForegroundColor { get; set; }
    public ConsoleThemeConfiguration? ConsoleTheme { get; set; }
    public ListViewItemThemeConfiguration? ListViewItemTheme { get; set; }
}