using FileTime.App.Core.Services;
using FileTime.Core.ContentAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.App.ContainerSizeScanner;

public static class Startup
{
    public static IServiceCollection AddContainerSizeScanner(this IServiceCollection services)
    {
        services.TryAddSingleton<IContainerScanSnapshotProvider, ContainerScanProvider>();
        services.AddSingleton<IContentProvider>(sp => sp.GetRequiredService<IContainerScanSnapshotProvider>());
        services.AddTransient<ISizeScanTask, SizeScanTask>();
        services.AddTransient<IItemPreviewProvider, PreviewProvider>();
        return services;
    }
}