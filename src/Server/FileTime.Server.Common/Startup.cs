using FileTime.Server.Common.Connections.SignalR;
using FileTime.Server.Common.ContentAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.Server.Common;

public static class Startup
{
    public static IServiceCollection AddServerCoreServices(this IServiceCollection services)
    {
        services.AddTransient<SignalRConnection>();
        services.TryAddSingleton<IApplicationStopper, ApplicationStopper>();
        services.AddSingleton<IContentAccessManager, ContentAccessManager>();
        return services;
    }
}