using FileTime.App.CommandPalette.Services;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.App.Core.ViewModels.Timeline;
using FileTime.App.FrequencyNavigation.Services;
using FileTime.GuiApp.App.Services;
using FileTime.Providers.LocalAdmin;

namespace FileTime.GuiApp.App.ViewModels;

public interface IMainWindowViewModel : IMainWindowViewModelBase
{
    IGuiAppState AppState { get; }
    IItemPreviewService ItemPreviewService { get; }
    IDialogService DialogService { get; }
    IFrequencyNavigationService FrequencyNavigationService { get; }
    ICommandPaletteService CommandPaletteService { get; }
    IRefreshSmoothnessCalculator RefreshSmoothnessCalculator { get; }
    IAdminElevationManager AdminElevationManager { get; }
    IClipboardService ClipboardService { get; }
    ITimelineViewModel TimelineViewModel { get; }
    IPossibleCommandsViewModel PossibleCommands { get; }
    Action? ShowWindow { get; set; }
    Task RunOrOpenItem(IItemViewModel itemViewModel);
}