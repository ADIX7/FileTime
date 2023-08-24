using FileTime.App.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.Providers.LocalAdmin;

public static class Startup
{
    public static IServiceCollection AddLocalAdminProviderServices(this IServiceCollection services, IConfigurationRoot configuration)
    {
        services.AddOptions<AdminElevationConfiguration>().Bind(configuration.GetSection(AdminElevationConfiguration.SectionName));
        services.TryAddSingleton<IAdminContentAccessorFactory, AdminContentAccessorFactory>();
        services.TryAddSingleton<AdminElevationManager>();
        services.TryAddSingleton<IAdminElevationManager>(sp => sp.GetRequiredService<AdminElevationManager>());
        services.AddSingleton<IExitHandler>(sp => sp.GetRequiredService<AdminElevationManager>());
        return services;
    }
}