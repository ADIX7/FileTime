using FileTime.App.Core.Models;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;

namespace FileTime.ConsoleUI.App.KeyInputHandling;

public class KeyInputHandlerService : IKeyInputHandlerService
{
    private readonly IAppState _appState;
    private readonly IDefaultModeKeyInputHandler _defaultModeKeyInputHandler;
    private readonly IRapidTravelModeKeyInputHandler _rapidTravelModeKeyInputHandler;
    bool _isCtrlPressed = false;
    bool _isShiftPressed = false;
    bool _isAltPressed = false;

    public KeyInputHandlerService(
        IAppState appState,
        IDefaultModeKeyInputHandler defaultModeKeyInputHandler,
        IRapidTravelModeKeyInputHandler rapidTravelModeKeyInputHandler)
    {
        _appState = appState;
        _defaultModeKeyInputHandler = defaultModeKeyInputHandler;
        _rapidTravelModeKeyInputHandler = rapidTravelModeKeyInputHandler;
    }

    public void HandleKeyInput(GeneralKeyEventArgs keyEvent)
    {
        var specialKeysStatus = new SpecialKeysStatus(_isAltPressed, _isShiftPressed, _isCtrlPressed);
        if (_appState.ViewMode.Value == ViewMode.Default)
        {
            Task.Run(async () => await _defaultModeKeyInputHandler.HandleInputKey(keyEvent, specialKeysStatus)).Wait();
        }
        else
        {
            Task.Run(async () => await _rapidTravelModeKeyInputHandler.HandleInputKey(keyEvent, specialKeysStatus)).Wait();
        }
    }
}