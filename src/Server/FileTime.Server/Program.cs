using Autofac;
using Autofac.Extensions.DependencyInjection;
using FileTime.App.DependencyInjection;
using FileTime.Providers.Local;
using FileTime.Server.App;
using FileTime.Server.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;


var applicationCancellation = new CancellationTokenSource();
var configurationRoot = CreateConfiguration();

Log.Logger = new LoggerConfiguration()
#if DEBUG
    .MinimumLevel.Verbose()
    .ReadFrom.Configuration(configurationRoot)
#else
    .MinimumLevel.Information()
#endif
    .WriteTo.Console()
    .CreateLogger();

var rootContainer = CreateRootDiContainer(configurationRoot);

var handlerParameters = new ConnectionHandlerParameters(
    args,
    rootContainer,
    configurationRoot,
    applicationCancellation.Token
);

var webThread = CreateStartup(FileTime.Server.Web.Program.Start);
webThread.Start();

Thread CreateStartup(Func<ConnectionHandlerParameters, Task> startup)
{
    var thread = new Thread(() => { HandleStartup(() => startup(handlerParameters).Wait()); });
    return thread;
}

void HandleStartup(Action action)
{
    try
    {
        action();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }
}

IConfigurationRoot CreateConfiguration()
{
    var configurationBuilder = new ConfigurationBuilder();
    configurationBuilder.AddCommandLine(args);
#if DEBUG
    configurationBuilder.AddJsonFile("appsettings.Development.json", optional: true);
    configurationBuilder.AddJsonFile("appsettings.Local.json", optional: true);
#endif
    return configurationBuilder.Build();
}

IContainer CreateRootDiContainer(IConfigurationRoot configuration)
{
    var serviceCollection = DependencyInjection
        .RegisterDefaultServices(configuration)
        .AddLocalProviderServices()
        .AddServerServices()
        .AddServerCoreServices()
        .AddLogging(loggingBuilder => loggingBuilder.AddSerilog());

    serviceCollection.AddSingleton<IApplicationStopper>(
        new ApplicationStopper(() => applicationCancellation.Cancel())
    );

    var containerBuilder = new ContainerBuilder();
    containerBuilder.Populate(serviceCollection);
    return containerBuilder.Build();
}