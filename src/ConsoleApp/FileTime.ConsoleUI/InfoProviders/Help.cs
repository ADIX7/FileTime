using System.Text;
using FileTime.App.Core.Configuration;
using FileTime.ConsoleUI.App.Configuration;

namespace FileTime.ConsoleUI.InfoProviders;

public static class Help
{
    public static void PrintHelp(IEnumerable<string> infoProvidersKeys)
    {
        StringBuilder sb = new();
        
        sb.AppendLine("Options:");
        PrintDriverOption(sb);
        sb.AppendLine();
        sb.AppendLine("Info providers:");
        foreach (var infoProviderKey in infoProvidersKeys.Order())
        {
            sb.AppendLine("\t" + infoProviderKey);
        }
        
        Console.Write(sb.ToString());
    }

    private static void PrintDriverOption(StringBuilder sb)
    {
        sb.AppendLine($"--{SectionNames.ApplicationSectionName}.{nameof(ConsoleApplicationConfiguration.ConsoleDriver)}");
        foreach (var driver in Startup.Drivers.Keys.Order())
        {
            sb.AppendLine("\t" + driver);
        }
    }
}