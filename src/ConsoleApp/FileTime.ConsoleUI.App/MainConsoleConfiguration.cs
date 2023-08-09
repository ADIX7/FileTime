namespace FileTime.ConsoleUI.App;

public class MainConsoleConfiguration
{
    public static Dictionary<string, string?> Configuration { get; }
    static MainConsoleConfiguration()
    {
        Configuration = new();
    }
}