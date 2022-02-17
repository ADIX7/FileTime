using FileTime.Core.Providers;
using FileTime.Providers.Smb.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Providers.Smb
{
    public static class Startup
    {
        public static IServiceCollection AddSmbServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<PersistenceService>()
                .AddSingleton<IContentProvider, SmbContentProvider>();
        }
    }
}