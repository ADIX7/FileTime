using System.Collections.ObjectModel;
using FileTime.App.Core.ViewModels;
using FileTime.GuiApp.App.Models;

namespace FileTime.GuiApp.App.ViewModels;

public interface IGuiAppState : IAppState
{
    ObservableCollection<RootDriveInfo> RootDriveInfos { get; set; }
    IReadOnlyList<PlaceInfo> Places { get; set; }
    ObservableCollection<string> PopupTexts { get; }
    IObservable<GuiPanel> ActivePanel { get; }

    void SetActivePanel(GuiPanel newPanel);
}
