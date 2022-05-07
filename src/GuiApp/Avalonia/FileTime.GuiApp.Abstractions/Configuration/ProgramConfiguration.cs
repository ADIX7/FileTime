namespace FileTime.GuiApp.Configuration;

public class ProgramConfiguration
{
    public string? Path { get; set; }
    public string? Arguments { get; set; }

    public ProgramConfiguration() { }

    public ProgramConfiguration(string? path, string? arguments = null)
    {
        Path = path;
        Arguments = arguments;
    }
}