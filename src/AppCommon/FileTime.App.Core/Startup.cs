using FileTime.App.Core.Services;
using FileTime.App.Core.Services.UserCommandHandler;
using FileTime.App.Core.StartupServices;
using FileTime.App.Core.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FileTime.App.Core;

public static class Startup
{
    public static IServiceCollection AddCoreAppServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddTransient<ITabViewModel, TabViewModel>()
            .AddTransient<IContainerViewModel, ContainerViewModel>()
            .AddTransient<IElementViewModel, ElementViewModel>()
            .AddTransient<IItemNameConverterService, ItemNameConverterService>()
            .AddSingleton<IUserCommandHandlerService, UserCommandHandlerService>()
            .AddSingleton<IClipboardService, ClipboardService>()
            .AddSingleton<IIdentifiableUserCommandService, IdentifiableUserCommandService>()
            .AddSingleton<IStartupHandler, DefaultIdentifiableCommandHandlerRegister>()
            .AddCommandHandlers();
    }

    private static IServiceCollection AddCommandHandlers(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IUserCommandHandler, NavigationUserCommandHandlerService>()
            .AddSingleton<IUserCommandHandler, ItemManipulationUserCommandHandlerService>();
    }
}