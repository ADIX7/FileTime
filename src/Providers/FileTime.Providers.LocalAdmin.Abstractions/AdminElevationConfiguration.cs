namespace FileTime.Providers.LocalAdmin;

public class AdminElevationConfiguration
{
    public const string SectionName = "AdminElevation";
    public string ServerExecutablePath { get; set; }
    public string LinuxElevationTool { get; set; }
    public int? ServerPort { get; set; }
    public bool? StartProcess { get; set; }
}