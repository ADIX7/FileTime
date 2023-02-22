namespace FileTime.GuiApp.Configuration;

public class FontConfiguration
{
    public const string SectionName = "Font";
    
    public List<string> Main { get; set; } = new();
    public List<string> DateTime { get; set; } = new();
}