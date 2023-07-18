using FileTime.App.CommandPalette.Services;
using FileTime.App.Core.Services;
using FileTime.App.FrequencyNavigation.Services;
using FileTime.GuiApp.Services;

namespace FileTime.GuiApp.ViewModels;

public interface IMainWindowViewModel : IMainWindowViewModelBase
{
    string Title { get; }
    IGuiAppState AppState { get; }
    IItemPreviewService ItemPreviewService { get; }
    IDialogService DialogService { get; }
    IFrequencyNavigationService FrequencyNavigationService { get; }
    ICommandPaletteService CommandPaletteService { get; }
}