using System.Text;
using FileTime.App.Core.Configuration;
using FileTime.ConsoleUI.App.Configuration;

namespace FileTime.ConsoleUI;

public static class Help
{
    public static void PrintHelp()
    {
        StringBuilder sb = new();
        
        sb.AppendLine("Options:");
        PrintDriverOption(sb);
        Console.Write(sb.ToString());
    }

    public static void PrintDriverOption(StringBuilder sb)
    {
        sb.AppendLine($"--{SectionNames.ApplicationSectionName}.{nameof(ConsoleApplicationConfiguration.ConsoleDriver)}");
        foreach (var driver in Startup.Drivers.Keys)
        {
            sb.AppendLine("\t" + driver);
        }
    }
}