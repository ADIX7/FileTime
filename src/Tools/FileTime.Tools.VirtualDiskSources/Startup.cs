using FileTime.App.Core.Services;
using FileTime.Core.ContentAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.Tools.VirtualDiskSources;

public static class Startup
{
    public static IServiceCollection AddVirtualDisk(this IServiceCollection services)
    {
        services.TryAddSingleton<IVirtualDiskSubContentProvider, VirtualDiskSubContentProvider>();
        services.AddSingleton<ISubContentProvider>(sp => sp.GetRequiredService<IVirtualDiskSubContentProvider>());
        services.AddSingleton<IPreStartupHandler, DiscUtilsInitializer>();
        services.TryAddSingleton<IVirtualDiskContentProviderFactory, VirtualDiskContentProviderFactory>();
        services.TryAddSingleton<IContentReaderFactory<VirtualDiskContentProvider>, VirtualDiskContentReaderFactory>();
        return services;
    }
}