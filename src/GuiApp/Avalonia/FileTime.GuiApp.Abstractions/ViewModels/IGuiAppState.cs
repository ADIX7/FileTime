using FileTime.App.Core.Models;
using FileTime.App.Core.ViewModels;
using FileTime.GuiApp.Configuration;

namespace FileTime.GuiApp.ViewModels;

public interface IGuiAppState : IAppState
{
    List<KeyConfig> PreviousKeys { get; }
    bool IsAllShortcutVisible { get; set; }
    bool NoCommandFound { get; set; }
    string? MessageBoxText { get; set; }
    List<CommandBindingConfiguration> PossibleCommands { get; set; }
    BindedCollection<RootDriveInfo, string> RootDriveInfos { get; set; }
}