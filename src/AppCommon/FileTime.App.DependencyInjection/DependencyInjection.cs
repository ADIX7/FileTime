using FileTime.App.Core;
using FileTime.Core.Services;
using FileTime.Providers.Local;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.App.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection RegisterDefaultServices(IServiceCollection? serviceCollection = null)
        {
            serviceCollection ??= new ServiceCollection();

            return serviceCollection
                .AddTransient<ITab, Tab>()
                .AddCoreAppServices()
                .AddLocalServices();
        }
    }
}