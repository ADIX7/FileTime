namespace FileTime.GuiApp.App.Settings;

public interface ISettingsViewModel
{
    SettingsPane SelectedPane { get; set; }
    bool SetAsDefaultIsChecked { get; set; }
    List<SettingsPaneItem> PaneItems { get; }
    SettingsPaneItem SelectedPaneItem { get; set; }
    bool SetAsWinEHandlerIsChecked { get; set; }
}