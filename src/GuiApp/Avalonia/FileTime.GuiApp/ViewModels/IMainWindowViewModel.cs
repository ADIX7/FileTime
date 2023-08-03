using FileTime.App.CommandPalette.Services;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.App.FrequencyNavigation.Services;
using FileTime.GuiApp.Services;
using FileTime.Providers.LocalAdmin;

namespace FileTime.GuiApp.ViewModels;

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
    Task RunOrOpenItem(IItemViewModel itemViewModel);
}