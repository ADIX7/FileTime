using System.Runtime.InteropServices;
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
            serviceCollection = serviceCollection
                .AddLogging()
                .AddSingleton<ItemNameConverterService>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                serviceCollection.AddSingleton<IContextMenuProvider, WindowsContextMenuProvider>();
            }
            else
            {
                throw new System.Exception("TODO: implement linux contextmenu provider");
            }

            return serviceCollection;
        }
    }
}
