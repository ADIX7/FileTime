namespace FileTime.GuiApp.App.Settings;

public interface IDefaultBrowserRegister
{
    void RegisterAsDefaultEditor();
    void UnregisterAsDefaultEditor();

    void RegisterWinEShortcut();
    void UnregisterWinEShortcut();
    bool IsWinEShortcut();
    bool IsDefaultFileBrowser();
}