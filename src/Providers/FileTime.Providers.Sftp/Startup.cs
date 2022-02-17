using FileTime.Core.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Providers.Sftp
{
    public static class Startup
    {
        public static IServiceCollection AddSftpServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<IContentProvider, SftpContentProvider>();
        }
    }
}