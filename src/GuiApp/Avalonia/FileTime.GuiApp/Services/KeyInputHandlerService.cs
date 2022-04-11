using Avalonia.Input;
using FileTime.App.Core.Models.Enums;
using FileTime.GuiApp.Configuration;
using FileTime.GuiApp.Models;
using FileTime.GuiApp.ViewModels;

namespace FileTime.GuiApp.Services
{
    public class KeyInputHandlerService : IKeyInputHandlerService
    {
        private readonly IGuiAppState _appState;
        private readonly IDefaultModeKeyInputHandler _defaultModeKeyInputHandler;
        private readonly IRapidTravelModeKeyInputHandler _rapidTravelModeKeyInputHandler;

        public KeyInputHandlerService(
            IGuiAppState appState,
            IDefaultModeKeyInputHandler defaultModeKeyInputHandler,
            IRapidTravelModeKeyInputHandler rapidTravelModeKeyInputHandler
        )
        {
            _appState = appState;
            _defaultModeKeyInputHandler = defaultModeKeyInputHandler;
            _rapidTravelModeKeyInputHandler = rapidTravelModeKeyInputHandler;
        }

        public async Task ProcessKeyDown(Key key, KeyModifiers keyModifiers, Action<bool> setHandled)
        {
            if (key == Key.LeftAlt
                || key == Key.RightAlt
                || key == Key.LeftShift
                || key == Key.RightShift
                || key == Key.LeftCtrl
                || key == Key.RightCtrl)
            {
                return;
            }

            //_appState.NoCommandFound = false;

            var isAltPressed = (keyModifiers & KeyModifiers.Alt) == KeyModifiers.Alt;
            var isShiftPressed = (keyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift;
            var isCtrlPressed = (keyModifiers & KeyModifiers.Control) == KeyModifiers.Control;

            var specialKeyStatus = new SpecialKeysStatus(isAltPressed, isShiftPressed, isCtrlPressed);

            if (_appState.ViewMode == ViewMode.Default)
            {
                await _defaultModeKeyInputHandler.HandleInputKey(key, specialKeyStatus, setHandled);
            }
            else
            {
                await _rapidTravelModeKeyInputHandler.HandleInputKey(key, specialKeyStatus, setHandled);
            }
        }
    }
}