using FileTime.App.Core.Configuration;
using FileTime.App.Core.Services;
using FileTime.App.Core.Services.UserCommandHandler;
using FileTime.App.Core.StartupServices;
using FileTime.App.Core.ViewModels;
using FileTime.App.Core.ViewModels.ItemPreview;
using FileTime.App.Core.ViewModels.Timeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileTime.App.Core;

public static class Startup
{
    public static IServiceCollection AddAppCoreDependencies(this IServiceCollection serviceCollection, IConfigurationRoot configuration)
    {
        serviceCollection.TryAddTransient<ITabViewModel, TabViewModel>();
        serviceCollection.TryAddTransient<IContainerViewModel, ContainerViewModel>();
        serviceCollection.TryAddTransient<IElementViewModel, ElementViewModel>();
        serviceCollection.TryAddTransient<IItemNameConverterService, ItemNameConverterService>();
        serviceCollection.TryAddTransient<ElementPreviewViewModel>();
        serviceCollection.TryAddSingleton<IUserCommandHandlerService, UserCommandHandlerService>();
        serviceCollection.TryAddSingleton<IClipboardService, ClipboardService>();
        serviceCollection.TryAddSingleton<IIdentifiableUserCommandService, IdentifiableUserCommandService>();
        serviceCollection.TryAddSingleton<IItemPreviewService, ItemPreviewService>();
        serviceCollection.TryAddSingleton<ITimelineViewModel, TimelineViewModel>();
        serviceCollection.TryAddSingleton<IRefreshSmoothnessCalculator, RefreshSmoothnessCalculator>();
        serviceCollection.TryAddSingleton<IItemPreviewProvider, ElementPreviewProvider>();
        serviceCollection.TryAddSingleton<ILifecycleService, LifecycleService>();
        serviceCollection.TryAddSingleton<IModalService, ModalService>();
        serviceCollection.TryAddSingleton<IDefaultModeKeyInputHandler, DefaultModeKeyInputHandler>();
        serviceCollection.TryAddSingleton<IRapidTravelModeKeyInputHandler, RapidTravelModeKeyInputHandler>();
        serviceCollection.TryAddSingleton<IKeyboardConfigurationService, KeyboardConfigurationService>();
        serviceCollection.TryAddSingleton<ICommandKeysHelperService, CommandKeysHelperService>();
        serviceCollection.TryAddSingleton<IPossibleCommandsService, PossibleCommandsService>();
        serviceCollection.TryAddSingleton<IPossibleCommandsViewModel, PossibleCommandsViewModel>();
        serviceCollection.TryAddSingleton<IProgramsService, ProgramsService>();

        return serviceCollection
            .AddCommandHandlers()
            .AddConfiguration(configuration)
            .AddSingleton<IStartupHandler, DefaultIdentifiableCommandHandlerRegister>()
            .AddSingleton<IExitHandler, ContainerRefreshHandler>();
    }

    private static IServiceCollection AddCommandHandlers(this IServiceCollection serviceCollection) =>
        serviceCollection
            .AddSingleton<IUserCommandHandler, NavigationUserCommandHandlerService>()
            .AddSingleton<IUserCommandHandler, ItemManipulationUserCommandHandlerService>()
            .AddSingleton<IUserCommandHandler, ToolUserCommandHandlerService>()
            .AddSingleton<IUserCommandHandler, CommandSchedulerUserCommandHandlerService>();

    internal static IServiceCollection AddConfiguration(this IServiceCollection serviceCollection, IConfigurationRoot configuration) =>
        serviceCollection
            .Configure<ProgramsConfigurationRoot>(configuration.GetSection(SectionNames.ProgramsSectionName))
            .Configure<KeyBindingConfiguration>(configuration.GetSection(SectionNames.KeybindingSectionName))
            .AddSingleton<IConfiguration>(configuration);
}