using FileTime.App.CommandPalette.ViewModels;
using FileTime.App.Core.Services;
using FileTime.App.Core.Services.UserCommandHandler;
using FileTime.App.Core.StartupServices;
using FileTime.App.Core.ViewModels;
using FileTime.App.Core.ViewModels.ItemPreview;
using FileTime.App.Core.ViewModels.Timeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.App.Core;

public static class Startup
{
    public static IServiceCollection AddCoreAppServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddTransient<ITabViewModel, TabViewModel>();
        serviceCollection.TryAddTransient<IContainerViewModel, ContainerViewModel>();
        serviceCollection.TryAddTransient<IElementViewModel, ElementViewModel>();
        serviceCollection.TryAddTransient<IFileViewModel, FileViewModel>();
        serviceCollection.TryAddTransient<IContainerSizeContainerViewModel, ContainerSizeContainerViewModel>();
        serviceCollection.TryAddTransient<IItemNameConverterService, ItemNameConverterService>();
        serviceCollection.TryAddTransient<ElementPreviewViewModel>();
        serviceCollection.TryAddSingleton<IUserCommandHandlerService, UserCommandHandlerService>();
        serviceCollection.TryAddSingleton<IClipboardService, ClipboardService>();
        serviceCollection.TryAddSingleton<IIdentifiableUserCommandService, IdentifiableUserCommandService>();
        serviceCollection.TryAddSingleton<IItemPreviewService, ItemPreviewService>();
        serviceCollection.TryAddSingleton<ITimelineViewModel, TimelineViewModel>();
        serviceCollection.TryAddSingleton<IRefreshSmoothnessCalculator, RefreshSmoothnessCalculator>();
        serviceCollection.TryAddSingleton<IItemPreviewProvider, ElementPreviewProvider>();

        return serviceCollection
            .AddCommandHandlers()
            .AddSingleton<IStartupHandler, DefaultIdentifiableCommandHandlerRegister>()
            .AddSingleton<IExitHandler, ContainerRefreshHandler>();
    }

    private static IServiceCollection AddCommandHandlers(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IUserCommandHandler, NavigationUserCommandHandlerService>()
            .AddSingleton<IUserCommandHandler, ItemManipulationUserCommandHandlerService>()
            .AddSingleton<IUserCommandHandler, ToolUserCommandHandlerService>()
            .AddSingleton<IUserCommandHandler, CommandSchedulerUserCommandHandlerService>();
    }
}