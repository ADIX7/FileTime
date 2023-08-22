namespace FileTime.App.Core.Configuration;

public class ProgramsConfigurationRoot
{
    public ProgramsConfiguration Linux { get; set; } = new();
    public ProgramsConfiguration Windows { get; set; } = new();
}