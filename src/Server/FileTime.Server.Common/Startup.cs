using FileTime.Server.Common.Connections.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Server.Common;

public static class Startup
{
    public static IServiceCollection AddRemoteServices(this IServiceCollection services)
    {
        services.AddTransient<SignalRConnection>();
        services.AddSingleton<IApplicationStopper, ApplicationStopper>();
        return services;
    }
}