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
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

var bootstrapConfiguration = CreateConfiguration();

var rootContainer = CreateRootDiContainer(bootstrapConfiguration);

var webThread = CreateStartup(FileTime.Server.Web.Program.Start);
webThread.Start();

Thread CreateStartup(Func<string[], IContainer, CancellationToken, Task> startup)
{
    var thread = new Thread(() => { HandleStartup(() => startup(args, rootContainer, applicationCancellation.Token).Wait()); });
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
    return configurationBuilder.Build();
}

IContainer CreateRootDiContainer(IConfigurationRoot configuration)
{
    var serviceCollection = DependencyInjection
        .RegisterDefaultServices(configuration)
        .AddLocalProviderServices()
        .AddServerServices()
        .AddLogging(loggingBuilder => loggingBuilder.AddSerilog());

    serviceCollection.TryAddSingleton<IApplicationStopper>(
        new ApplicationStopper(() => applicationCancellation.Cancel())
    );

    var containerBuilder = new ContainerBuilder();
    containerBuilder.Populate(serviceCollection);
    return containerBuilder.Build();
}