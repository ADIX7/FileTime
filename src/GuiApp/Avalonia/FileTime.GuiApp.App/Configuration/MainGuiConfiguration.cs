using FileTime.Providers.LocalAdmin;

namespace FileTime.GuiApp.App.Configuration;

public class MainGuiConfiguration
{
    
    public static Dictionary<string, string?> Configuration { get; }
    static MainGuiConfiguration()
    {
        Configuration = new()
        {
            {
                AdminElevationConfiguration.SectionName + ":" + nameof(AdminElevationConfiguration.LinuxElevationTool), "pkexec"
            },
        };
    }
}