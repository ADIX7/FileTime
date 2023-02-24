using FileTime.App.Core.Services;
using FileTime.App.FrequencyNavigation.Services;
using FileTime.App.FrequencyNavigation.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.App.FrequencyNavigation;

public static class Startup
{
    public static IServiceCollection AddFrequencyNavigation(this IServiceCollection services)
    {
        services.TryAddTransient<IFrequencyNavigationViewModel, FrequencyNavigationViewModel>();
        services.AddSingleton<FrequencyNavigationService>();
        services.TryAddSingleton<IFrequencyNavigationService>(sp => sp.GetRequiredService<FrequencyNavigationService>());
        services.AddSingleton<IStartupHandler>(sp => sp.GetRequiredService<FrequencyNavigationService>());
        services.AddSingleton<IExitHandler>(sp => sp.GetRequiredService<FrequencyNavigationService>());
        return services;
    }
}