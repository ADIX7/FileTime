using FileTime.ConsoleUI;
using FileTime.ConsoleUI.App;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
#if DEBUG
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
#endif
    .Build();
DI.Initialize(configuration);

var app = DI.ServiceProvider.GetRequiredService<IApplication>();
app.Run();