using Avalonia.Input;
using FileTime.App.Core.Models.Enums;
using FileTime.GuiApp.Models;
using FileTime.GuiApp.ViewModels;

namespace FileTime.GuiApp.Services;

public class KeyInputHandlerService : IKeyInputHandlerService
{
    private readonly IGuiAppState _appState;
    private readonly IDefaultModeKeyInputHandler _defaultModeKeyInputHandler;
    private readonly IRapidTravelModeKeyInputHandler _rapidTravelModeKeyInputHandler;
    private ViewMode _viewMode;
    private GuiPanel _activePanel;
    private readonly Dictionary<(GuiPanel, Key), GuiPanel> _panelMovements = new()
    {
        [(GuiPanel.FileBrowser, Key.Up)] = GuiPanel.Timeline,
        [(GuiPanel.Timeline, Key.Down)] = GuiPanel.FileBrowser,
    };

    public KeyInputHandlerService(
        IGuiAppState appState,
        IDefaultModeKeyInputHandler defaultModeKeyInputHandler,
        IRapidTravelModeKeyInputHandler rapidTravelModeKeyInputHandler
    )
    {
        _appState = appState;
        _defaultModeKeyInputHandler = defaultModeKeyInputHandler;
        _rapidTravelModeKeyInputHandler = rapidTravelModeKeyInputHandler;

        appState.ViewMode.Subscribe(v => _viewMode = v);
        appState.ActivePanel.Subscribe(p => _activePanel = p);
    }

    public async Task ProcessKeyDown(Key key, KeyModifiers keyModifiers, Action<bool> setHandled)
    {
        if (key is Key.LeftAlt
            or Key.RightAlt
            or Key.LeftShift
            or Key.RightShift
            or Key.LeftCtrl
            or Key.RightCtrl)
        {
            return;
        }

        //_appState.NoCommandFound = false;

        var isAltPressed = (keyModifiers & KeyModifiers.Alt) == KeyModifiers.Alt;
        var isShiftPressed = (keyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift;
        var isCtrlPressed = (keyModifiers & KeyModifiers.Control) == KeyModifiers.Control;

        if (isCtrlPressed
                && key is Key.Left or Key.Right or Key.Up or Key.Down
                && _panelMovements.TryGetValue((_activePanel, key), out var newPanel))
        {
            _appState.SetActivePanel(newPanel);
            setHandled(true);
            return;
        }

        var specialKeyStatus = new SpecialKeysStatus(isAltPressed, isShiftPressed, isCtrlPressed);

        if (_activePanel == GuiPanel.FileBrowser)
        {
            if (_viewMode == ViewMode.Default)
            {
                await _defaultModeKeyInputHandler.HandleInputKey(key, specialKeyStatus, setHandled);
            }
            else
            {
                await _rapidTravelModeKeyInputHandler.HandleInputKey(key, specialKeyStatus, setHandled);
            }
        }
        else if (_activePanel == GuiPanel.Timeline)
        {
            // await HandleTimelineKey(key, specialKeyStatus, setHandled);
        }
        else if (_activePanel == GuiPanel.Drives)
        {
            // await HandleDrivesKey(key, specialKeyStatus, setHandled);
        }
        else if (_activePanel == GuiPanel.Favorites)
        {
            // await HandleFavoritesKey(key, specialKeyStatus, setHandled);
        }
    }
}
