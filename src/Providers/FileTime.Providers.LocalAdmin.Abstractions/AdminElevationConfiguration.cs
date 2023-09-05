namespace FileTime.Providers.LocalAdmin;

public class AdminElevationConfiguration
{
    public const string SectionName = "AdminElevation";
    public string? ServerExecutablePath { get; set; }
    public string LinuxElevationTool { get; set; } = null!;
    public int? ServerPort { get; set; }
    public bool? StartProcess { get; set; }
}