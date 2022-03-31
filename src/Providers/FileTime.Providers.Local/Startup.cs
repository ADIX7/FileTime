using FileTime.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Providers.Local
{
    public static class Startup
    {
        public static IServiceCollection AddLocalServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<LocalContentProvider>()
                .AddSingleton<IContentProvider, LocalContentProvider>(sp => sp.GetService<LocalContentProvider>() ?? throw new Exception($"No {nameof(LocalContentProvider)} instance found"));
        }
    }
}