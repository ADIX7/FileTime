using FileTime.Avalonia.Application;
using FileTime.Avalonia.Services;
using FileTime.Avalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Avalonia
{
    internal static class Startup
    {
        internal static IServiceCollection AddViewModels(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<AppState>()
                .AddTransient<MainPageViewModel>();
        }
        internal static IServiceCollection AddServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddLogging()
                .AddSingleton<ItemNameConverterService>();
        }
    }
}
