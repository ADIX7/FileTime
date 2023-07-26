using System.Net;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace FileTime.Server.Web;

public class Program
{
    public static async Task Start(string[] args, IContainer rootContainer, CancellationToken applicationExit)
    {
        var builder = WebApplication.CreateBuilder(args);

        var configuration = builder.Configuration;

        builder.Host.UseServiceProviderFactory(
            new AutofacChildLifetimeScopeServiceProviderFactory(rootContainer.BeginLifetimeScope("WebScope"))
        );

        builder.Host.UseSerilog();
        builder.WebHost.ConfigureKestrel((buildContext, serverOptions) =>
        {
            var port = buildContext.Configuration.GetValue<int?>("WebPort") ?? 0;
            serverOptions.Listen(new IPEndPoint(IPAddress.Loopback, port));
        });

        builder.Services.AddSignalR();
        builder.Services.AddHealthChecks();
        builder.Services.AddHostedService<PortWriterService>();

        builder.Services.AddOptions<PortWriterConfiguration>()
            .Bind(configuration.GetSection(PortWriterConfiguration.SectionName));

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
        }

        app.MapHub<ConnectionHub>("/RemoteHub");
        app.UseHealthChecks("/health");

        await app.RunAsync(applicationExit);
    }
}