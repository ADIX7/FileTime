using FileTime.App.CommandPalette.Services;
using FileTime.App.CommandPalette.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.App.CommandPalette;

public static class Startup
{
    public static IServiceCollection AddCommandPalette(this IServiceCollection services)
    {
        services.TryAddTransient<ICommandPaletteViewModel, CommandPaletteViewModel>();
        services.TryAddSingleton<ICommandPaletteService, CommandPaletteService>();
        return services;
    }
}