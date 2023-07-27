using System.Net;
using Autofac.Extensions.DependencyInjection;
using FileTime.Server.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace FileTime.Server.Web;

public class Program
{
    public static async Task Start(ConnectionHandlerParameters parameters)
    {
        var builder = WebApplication.CreateBuilder(parameters.Args);

        var configuration = builder.Configuration;

        //Note: Use app wide configuration instead of the default ASP.NET Core configuration 
#pragma warning disable ASP0013
        builder.Host.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.Sources.Clear();
            configurationBuilder.AddConfiguration(parameters.ConfigurationRoot);
        });
#pragma warning restore ASP0013

        builder.Host.UseServiceProviderFactory(
            new AutofacChildLifetimeScopeServiceProviderFactory(parameters.RootContainer.BeginLifetimeScope("WebScope"))
        );

        builder.Host.UseSerilog();
        builder.WebHost.ConfigureKestrel((buildContext, serverOptions) =>
        {
            var port = buildContext.Configuration.GetValue<int?>("WebPort") ?? 0;
            serverOptions.Listen(new IPEndPoint(IPAddress.Loopback, port));
        });

        builder.Services.AddHttpLogging(options => options.LoggingFields = HttpLoggingFields.All);
        builder.Services.AddSignalR(hubOptions =>
        {
            hubOptions.MaximumReceiveMessageSize = 20 * 1024 * 1024; // 10MB
        });
        builder.Services.AddHealthChecks();
        builder.Services.AddHostedService<PortWriterService>();

        builder.Services.AddOptions<PortWriterConfiguration>()
            .Bind(configuration.GetSection(PortWriterConfiguration.SectionName));

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpLogging();
        }

        app.MapHub<ConnectionHub>("/RemoteHub");
        app.UseHealthChecks("/health");

        await app.RunAsync(parameters.ApplicationExit);
    }
}