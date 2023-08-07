using FileTime.App.Core;
using FileTime.App.Core.Configuration;
using FileTime.ConsoleUI;
using FileTime.ConsoleUI.App;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

(AppDataRoot, EnvironmentName) = Init.InitDevelopment();
var configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(MainConfiguration.Configuration)
#if DEBUG
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
#endif
    .Build();
DI.Initialize(configuration);

var app = DI.ServiceProvider.GetRequiredService<IApplication>();
app.Run();

public partial class Program
{
    public static string AppDataRoot { get; private set; }
    public static string EnvironmentName { get; private set; }
}