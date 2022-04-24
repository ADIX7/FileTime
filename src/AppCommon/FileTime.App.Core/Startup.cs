using FileTime.App.Core.Services;
using FileTime.App.Core.Services.CommandHandler;
using FileTime.App.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.App.Core
{
    public static class Startup
    {
        public static IServiceCollection AddCoreAppServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddTransient<ITabViewModel, TabViewModel>()
                .AddTransient<IContainerViewModel, ContainerViewModel>()
                .AddTransient<IElementViewModel, ElementViewModel>()
                .AddTransient<IItemNameConverterService, ItemNameConverterService>()
                .AddSingleton<ICommandHandlerService, CommandHandlerService>()
                .AddCommandHandlers();
        }

        private static IServiceCollection AddCommandHandlers(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<ICommandHandler, NavigationCommandHandler>()
                .AddSingleton<ICommandHandler, ItemManipulationCommandHandler>();
        }
    }
}