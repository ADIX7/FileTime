using DeclarativeProperty;
using FileTime.App.CommandPalette.ViewModels;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.App.Core.ViewModels.Timeline;
using FileTime.App.FrequencyNavigation.ViewModels;
using FileTime.ConsoleUI.App.Services;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using FileTime.Providers.LocalAdmin;

namespace FileTime.ConsoleUI.App;

public partial class RootViewModel : IRootViewModel
{
    public string UserName => Environment.UserName;
    public string MachineName => Environment.MachineName;
    public IPossibleCommandsViewModel PossibleCommands { get; }
    public IConsoleAppState AppState { get; }
    public ICommandPaletteViewModel CommandPalette { get; }
    public IFrequencyNavigationViewModel FrequencyNavigation { get; }
    public IItemPreviewService ItemPreviewService { get; }
    public IClipboardService ClipboardService { get; }
    public IAdminElevationManager AdminElevationManager { get; }
    public IDialogService DialogService { get; }
    public ITimelineViewModel TimelineViewModel { get; }
    public IDeclarativeProperty<VolumeSizeInfo?> VolumeSizeInfo { get;}
    
    public event Action<IInputElement>? FocusReadInputElement;

    public RootViewModel(
        IConsoleAppState appState,
        IPossibleCommandsViewModel possibleCommands,
        ICommandPaletteViewModel commandPalette,
        IDialogService dialogService,
        ITimelineViewModel timelineViewModel,
        IFrequencyNavigationViewModel frequencyNavigation,
        IItemPreviewService itemPreviewService,
        IClipboardService clipboardService,
        IAdminElevationManager adminElevationManager)
    {
        AppState = appState;
        PossibleCommands = possibleCommands;
        CommandPalette = commandPalette;
        DialogService = dialogService;
        TimelineViewModel = timelineViewModel;
        FrequencyNavigation = frequencyNavigation;
        ItemPreviewService = itemPreviewService;
        ClipboardService = clipboardService;
        AdminElevationManager = adminElevationManager;

        DialogService.ReadInput.PropertyChanged += (o, e) =>
        {
            if (e.PropertyName == nameof(DialogService.ReadInput.Value))
            {
                if (DialogService.ReadInput.Value is {Inputs.Count: > 0} readInputs)
                {
                    FocusReadInputElement?.Invoke(readInputs.Inputs[0]);
                }
            }
        };

        itemPreviewService.ItemPreview.PropertyChanged += (o, e) =>
        {
            if (e.PropertyName == nameof(itemPreviewService.ItemPreview.Value))
            {
                appState.PreviewType = null;
            }
        };

        VolumeSizeInfo = appState.SelectedTab
            .Map(t => t?.CurrentLocation)
            .Switch()
            .Map(l => l?.Provider.GetVolumeSizeInfo(l.FullName!));
    }
}