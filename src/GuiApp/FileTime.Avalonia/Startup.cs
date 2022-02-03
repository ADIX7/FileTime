using System.Runtime.InteropServices;
using FileTime.Avalonia.Application;
using FileTime.Avalonia.IconProviders;
using FileTime.Avalonia.Services;
using FileTime.Avalonia.ViewModels;
using FileTime.Core.Command;
using FileTime.Core.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.Avalonia
{
    internal static class Startup
    {
        internal static IServiceCollection AddViewModels(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<AppState>()
                .AddTransient<MainPageViewModel>()
                .AddSingleton<IInputInterface, BasicInputHandler>();
        }
        internal static IServiceCollection AddServices(this IServiceCollection serviceCollection)
        {
            serviceCollection = serviceCollection
                .AddLogging()
                .AddSingleton<ItemNameConverterService>()
                .AddSingleton<StatePersistenceService>()
                .AddSingleton<IIconProvider, MaterialIconProvider>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                serviceCollection.AddSingleton<IContextMenuProvider, WindowsContextMenuProvider>();
            }
            else
            {
                serviceCollection.AddSingleton<IContextMenuProvider, LinuxContextMenuProvider>();
            }

            return serviceCollection;
        }
        internal static IServiceCollection RegisterCommandHandlers(this IServiceCollection serviceCollection)
        {
            foreach (var commandHandler in FileTime.Providers.Local.Startup.GetCommandHandlers())
            {
                serviceCollection.AddTransient(typeof(ICommandHandler), commandHandler);
            }

            return serviceCollection;
        }
    }
}
