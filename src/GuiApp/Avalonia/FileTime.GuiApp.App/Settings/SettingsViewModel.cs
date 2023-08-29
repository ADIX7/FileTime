using System.ComponentModel;
using System.Runtime.InteropServices;
using PropertyChanged.SourceGenerator;

namespace FileTime.GuiApp.App.Settings;

public record SettingsPaneItem(string Header, SettingsPane Pane);

public partial class SettingsViewModel : ISettingsViewModel
{
    private readonly IDefaultBrowserRegister _defaultBrowserRegister;

    [Notify] private SettingsPane _selectedPane;
    [Notify] private bool _setAsDefaultIsChecked;
    [Notify] private bool _setAsWinEHandlerIsChecked;

    public bool ShowWindowsSpecificSettings => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public List<SettingsPaneItem> PaneItems { get; } = new()
    {
        new("Home", SettingsPane.Home),
        new("Advanced", SettingsPane.Advanced)
    };

    public SettingsPaneItem SelectedPaneItem
    {
        get => PaneItems.First(x => x.Pane == SelectedPane);
        set => SelectedPane = value.Pane;
    }

    public SettingsViewModel(IDefaultBrowserRegister defaultBrowserRegister)
    {
        _defaultBrowserRegister = defaultBrowserRegister;

        _setAsWinEHandlerIsChecked = defaultBrowserRegister.IsWinEShortcut();
        _setAsDefaultIsChecked = defaultBrowserRegister.IsDefaultFileBrowser();
        
        PropertyChanged += SettingsViewModel_PropertyChanged;
    }

    private void SettingsViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SetAsDefaultIsChecked))
        {
            if (SetAsDefaultIsChecked)
            {
                _defaultBrowserRegister.RegisterAsDefaultEditor();
            }
            else
            {
                _defaultBrowserRegister.UnregisterAsDefaultEditor();
            }
        }
        else if (e.PropertyName == nameof(SetAsWinEHandlerIsChecked))
        {
            if (SetAsWinEHandlerIsChecked)
            {
                _defaultBrowserRegister.RegisterWinEShortcut();
            }
            else
            {
                _defaultBrowserRegister.UnregisterWinEShortcut();
            }
        }
    }
}