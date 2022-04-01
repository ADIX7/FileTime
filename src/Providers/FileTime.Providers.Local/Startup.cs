using FileTime.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Providers.Local
{
    public static class Startup
    {
        public static IServiceCollection AddLocalServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<ILocalContentProvider, LocalContentProvider>()
                .AddSingleton<IContentProvider, ILocalContentProvider>(sp => sp.GetRequiredService<ILocalContentProvider>());
        }
    }
}