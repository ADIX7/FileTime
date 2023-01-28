using FileTime.Core.ContentAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.Providers.Local;

public static class Startup
{
    public static IServiceCollection AddLocalServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<ILocalContentProvider, LocalContentProvider>();
        serviceCollection.TryAddSingleton<IContentProvider>(sp => sp.GetRequiredService<ILocalContentProvider>());
        serviceCollection.TryAddSingleton<IItemCreator<ILocalContentProvider>, LocalItemCreator>();
        serviceCollection.TryAddSingleton<IItemCreator<LocalContentProvider>>(sp => sp.GetRequiredService<IItemCreator<ILocalContentProvider>>());
        serviceCollection.TryAddSingleton<IItemDeleter<ILocalContentProvider>, LocalItemDeleter>();
        serviceCollection.TryAddSingleton<IItemDeleter<LocalContentProvider>>(sp => sp.GetRequiredService<IItemDeleter<ILocalContentProvider>>());
        serviceCollection.TryAddSingleton<IContentReaderFactory<ILocalContentProvider>, LocalContentReaderFactory>();
        serviceCollection.TryAddSingleton<IContentReaderFactory<LocalContentProvider>>(sp => sp.GetRequiredService<IContentReaderFactory<ILocalContentProvider>>());
        serviceCollection.TryAddSingleton<IContentWriterFactory<ILocalContentProvider>, LocalContentWriterFactory>();
        serviceCollection.TryAddSingleton<IContentWriterFactory<LocalContentProvider>>(sp => sp.GetRequiredService<IContentWriterFactory<ILocalContentProvider>>());
        return serviceCollection;
    }
}