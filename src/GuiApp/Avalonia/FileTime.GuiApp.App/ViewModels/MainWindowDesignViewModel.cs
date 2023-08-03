using System.Reactive.Subjects;
using FileTime.App.CommandPalette.Services;
using FileTime.App.Core.Services;
using FileTime.App.FrequencyNavigation.Services;
using FileTime.GuiApp.DesignPreview.Services;
using FileTime.GuiApp.App.Services;

namespace FileTime.GuiApp.App.ViewModels;

public class MainWindowDesignViewModel //: IMainWindowViewModel
{
    public bool Loading => false;
    public IObservable<string?> MainFont { get; } = new BehaviorSubject<string?>(null);
    public string Title => "FileTime Design Preview";
    public IGuiAppState AppState { get; }
    public IItemPreviewService ItemPreviewService { get; }
    public IDialogService DialogService { get; }
    public IFrequencyNavigationService FrequencyNavigationService { get; }
    public ICommandPaletteService CommandPaletteService { get; }

    public MainWindowDesignViewModel()
    {
        //AppState = new GuiAppStatePreview();
    }
}