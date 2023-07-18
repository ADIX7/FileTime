using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.ViewModels;
using FileTime.App.Core.ViewModels.Timeline;
using FileTime.Core.Models;
using FileTime.GuiApp.Configuration;
using FileTime.GuiApp.Models;
using FileTime.GuiApp.ViewModels;

namespace FileTime.GuiApp.DesignPreview.Services;

public class GuiAppStatePreview : IGuiAppState
{
    public GuiAppStatePreview()
    {
        var tab = new TabViewModelPreview(this);
        SelectedTab = new BehaviorSubject<ITabViewModel>(tab);
        CurrentSelectedTab = tab;
        
        var tabs = new ObservableCollection<ITabViewModel>(new [] {tab});
        Tabs = new ReadOnlyObservableCollection<ITabViewModel>(tabs);

        SearchText = new BehaviorSubject<string?>(null);
        ViewMode = new BehaviorSubject<ViewMode>(FileTime.App.Core.Models.Enums.ViewMode.Default);

        PreviousKeys = new();

        ActivePanel = new BehaviorSubject<GuiPanel>(GuiPanel.FileBrowser);
        PopupTexts = new ObservableCollection<string>();
    }

    public ReadOnlyObservableCollection<ITabViewModel> Tabs { get; }
    public IObservable<ITabViewModel> SelectedTab { get; }
    public IObservable<string?> SearchText { get; }
    public IObservable<ViewMode> ViewMode { get; }
    public string RapidTravelText { get; set; }
    public ITabViewModel? CurrentSelectedTab { get; }
    public ITimelineViewModel TimelineViewModel { get; }
    public void SetSearchText(string? searchText) => throw new NotImplementedException();

    public void SetSelectedTab(ITabViewModel tabToSelect) => throw new NotImplementedException();

    public void SwitchViewMode(ViewMode newViewMode) => throw new NotImplementedException();

    public void RemoveTab(ITabViewModel tabViewModel) => throw new NotImplementedException();

    public void AddTab(ITabViewModel tabViewModel) => throw new NotImplementedException();

    public List<KeyConfig> PreviousKeys { get; }
    public bool IsAllShortcutVisible { get; set; }
    public bool NoCommandFound { get; set; }
    public List<CommandBindingConfiguration> PossibleCommands { get; set; }
    public BindedCollection<RootDriveInfo, string> RootDriveInfos { get; set; }
    public IReadOnlyList<PlaceInfo> Places { get; set; }
    public ObservableCollection<string> PopupTexts { get; }
    public IObservable<GuiPanel> ActivePanel { get; }
    public void SetActivePanel(GuiPanel newPanel) => throw new NotImplementedException();
}