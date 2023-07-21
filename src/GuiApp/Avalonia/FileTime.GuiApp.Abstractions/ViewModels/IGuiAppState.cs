using System.Collections.ObjectModel;
using DeclarativeProperty;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.GuiApp.Configuration;
using FileTime.GuiApp.Models;

namespace FileTime.GuiApp.ViewModels;

public interface IGuiAppState : IAppState
{
    List<KeyConfig> PreviousKeys { get; }
    bool IsAllShortcutVisible { get; set; }
    bool NoCommandFound { get; set; }
    List<CommandBindingConfiguration> PossibleCommands { get; set; }
    ObservableCollection<RootDriveInfo> RootDriveInfos { get; set; }
    IReadOnlyList<PlaceInfo> Places { get; set; }
    ObservableCollection<string> PopupTexts { get; }
    IObservable<GuiPanel> ActivePanel { get; }

    void SetActivePanel(GuiPanel newPanel);
}
