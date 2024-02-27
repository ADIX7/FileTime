using Avalonia.Input;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Services;
using FileTime.GuiApp.App.Extensions;
using FileTime.GuiApp.App.Models;
using FileTime.GuiApp.App.ViewModels;
using GeneralInputKey;

namespace FileTime.GuiApp.App.Services;

public class KeyInputHandlerService : IKeyInputHandlerService
{
    private readonly IGuiAppState _appState;
    private readonly IDefaultModeKeyInputHandler _defaultModeKeyInputHandler;
    private readonly IRapidTravelModeKeyInputHandler _rapidTravelModeKeyInputHandler;
    private readonly IAppKeyService<Key> _appKeyService;
    private GuiPanel _activePanel;

    public event EventHandler? UnhandledEsc;

    private readonly Dictionary<(GuiPanel, Key), GuiPanel> _panelMovements = new()
    {
        [(GuiPanel.FileBrowser, Key.Up)] = GuiPanel.Timeline,
        [(GuiPanel.Timeline, Key.Down)] = GuiPanel.FileBrowser,
    };

    public KeyInputHandlerService(
        IGuiAppState appState,
        IDefaultModeKeyInputHandler defaultModeKeyInputHandler,
        IRapidTravelModeKeyInputHandler rapidTravelModeKeyInputHandler,
        IAppKeyService<Key> appKeyService)
    {
        _appState = appState;
        _defaultModeKeyInputHandler = defaultModeKeyInputHandler;
        _rapidTravelModeKeyInputHandler = rapidTravelModeKeyInputHandler;
        _appKeyService = appKeyService;

        appState.ActivePanel.Subscribe(p => _activePanel = p);
    }

    public async Task ProcessKeyDown(KeyEventArgs e)
    {
        if (e.Key is Key.LeftAlt
            or Key.RightAlt
            or Key.LeftShift
            or Key.RightShift
            or Key.LeftCtrl
            or Key.RightCtrl)
        {
            return;
        }

        //_appState.NoCommandFound = false;

        var isAltPressed = (e.KeyModifiers & KeyModifiers.Alt) != 0;
        var isShiftPressed = (e.KeyModifiers & KeyModifiers.Shift) != 0;
        var isCtrlPressed = (e.KeyModifiers & KeyModifiers.Control) != 0;

        if (isCtrlPressed
            && e.Key is Key.Left or Key.Right or Key.Up or Key.Down
            && _panelMovements.TryGetValue((_activePanel, e.Key), out var newPanel))
        {
            _appState.SetActivePanel(newPanel);
            e.Handled = true;
            return;
        }

        var specialKeyStatus = new SpecialKeysStatus(isAltPressed, isShiftPressed, isCtrlPressed);

        if (_activePanel == GuiPanel.FileBrowser)
        {
            if (_appState.ViewMode.Value == ViewMode.Default)
            {
                if (e.ToGeneralKeyEventArgs(_appKeyService, specialKeyStatus) is { } args)
                {
                    await _defaultModeKeyInputHandler.HandleInputKey(args);

                    if (!args.Handled && args.Key == Keys.Escape)
                    {
                        UnhandledEsc?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            else
            {
                if (e.ToGeneralKeyEventArgs(_appKeyService, specialKeyStatus) is { } args)
                {
                    await _rapidTravelModeKeyInputHandler.HandleInputKey(args);
                }
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