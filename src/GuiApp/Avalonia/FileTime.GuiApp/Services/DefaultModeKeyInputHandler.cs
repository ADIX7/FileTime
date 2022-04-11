using System.Reactive.Linq;
using Avalonia.Input;
using FileTime.App.Core.Command;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.GuiApp.Configuration;
using FileTime.GuiApp.Extensions;
using FileTime.GuiApp.Models;
using FileTime.GuiApp.ViewModels;
using Microsoft.Extensions.Logging;

namespace FileTime.GuiApp.Services
{
    public class DefaultModeKeyInputHandler : IDefaultModeKeyInputHandler
    {
        private readonly IGuiAppState _appState;
        private readonly IKeyboardConfigurationService _keyboardConfigurationService;
        private readonly List<KeyConfig[]> _keysToSkip = new();
        private ITabViewModel? _selectedTab;
        private IContainer? _currentLocation;
        private readonly ILogger<DefaultModeKeyInputHandler> _logger;
        private readonly ICommandHandlerService _commandHandlerService;

        public DefaultModeKeyInputHandler(
            IGuiAppState appState,
            IKeyboardConfigurationService keyboardConfigurationService,
            ILogger<DefaultModeKeyInputHandler> logger,
            ICommandHandlerService commandHandlerService)
        {
            _appState = appState;
            _keyboardConfigurationService = keyboardConfigurationService;
            _logger = logger;
            _commandHandlerService = commandHandlerService;

            _appState.SelectedTab.Subscribe(t => _selectedTab = t);
            _appState.SelectedTab.Select(t => t == null ? Observable.Return<IContainer?>(null) : t.CurrentLocation!).Switch().Subscribe(l => _currentLocation = l);

            _keysToSkip.Add(new KeyConfig[] { new KeyConfig(Key.Up) });
            _keysToSkip.Add(new KeyConfig[] { new KeyConfig(Key.Down) });
            _keysToSkip.Add(new KeyConfig[] { new KeyConfig(Key.Tab) });
            _keysToSkip.Add(new KeyConfig[] { new KeyConfig(Key.PageDown) });
            _keysToSkip.Add(new KeyConfig[] { new KeyConfig(Key.PageUp) });
            _keysToSkip.Add(new KeyConfig[] { new KeyConfig(Key.F4, alt: true) });
            _keysToSkip.Add(new KeyConfig[] { new KeyConfig(Key.LWin) });
            _keysToSkip.Add(new KeyConfig[] { new KeyConfig(Key.RWin) });
        }

        public async Task HandleInputKey(Key key, SpecialKeysStatus specialKeysStatus, Action<bool> setHandled)
        {
            var keyWithModifiers = new KeyConfig(key, shift: specialKeysStatus.IsShiftPressed, alt: specialKeysStatus.IsAltPressed, ctrl: specialKeysStatus.IsCtrlPressed);
            _appState.PreviousKeys.Add(keyWithModifiers);

            var selectedCommandBinding = _keyboardConfigurationService.UniversalCommandBindings.FirstOrDefault(c => c.Keys.AreKeysEqual(_appState.PreviousKeys));
            selectedCommandBinding ??= _keyboardConfigurationService.CommandBindings.FirstOrDefault(c => c.Keys.AreKeysEqual(_appState.PreviousKeys));

            if (key == Key.Escape)
            {
                var doGeneralReset = false;
                if (_appState.PreviousKeys.Count > 1 || _appState.IsAllShortcutVisible || _appState.MessageBoxText != null)
                {
                    doGeneralReset = true;
                }
                /*else if (_currentLocation.Container.CanHandleEscape)
                {
                    var escapeResult = await _currentLocation.Container.HandleEscape();
                    if (escapeResult.NavigateTo != null)
                    {
                        setHandled(true);
                        _appState.PreviousKeys.Clear();
                        await _appState.SelectedTab.OpenContainer(escapeResult.NavigateTo);
                    }
                    else
                    {
                        if (escapeResult.Handled)
                        {
                            _appState.PreviousKeys.Clear();
                        }
                        else
                        {
                            doGeneralReset = true;
                        }
                    }
                }*/

                if (doGeneralReset)
                {
                    setHandled(true);
                    _appState.IsAllShortcutVisible = false;
                    _appState.MessageBoxText = null;
                    _appState.PreviousKeys.Clear();
                    _appState.PossibleCommands = new();
                }
            }
            else if (key == Key.Enter
                && _appState.MessageBoxText != null)
            {
                _appState.PreviousKeys.Clear();
                //_dialogService.ProcessMessageBox();
                setHandled(true);
            }
            else if (selectedCommandBinding != null)
            {
                setHandled(true);
                _appState.PreviousKeys.Clear();
                _appState.PossibleCommands = new();
                await CallCommandAsync(selectedCommandBinding.Command);
            }
            else if (_keysToSkip.Any(k => k.AreKeysEqual(_appState.PreviousKeys)))
            {
                _appState.PreviousKeys.Clear();
                _appState.PossibleCommands = new();
                return;
            }
            else if (_appState.PreviousKeys.Count == 2)
            {
                setHandled(true);
                _appState.NoCommandFound = true;
                _appState.PreviousKeys.Clear();
                _appState.PossibleCommands = new();
            }
            else
            {
                setHandled(true);
                var possibleCommands = _keyboardConfigurationService.AllShortcut.Where(c => c.Keys[0].AreEquals(keyWithModifiers)).ToList();

                if (possibleCommands.Count == 0)
                {
                    _appState.NoCommandFound = true;
                    _appState.PreviousKeys.Clear();
                }
                else
                {
                    _appState.PossibleCommands = possibleCommands;
                }
            }
        }

        private async Task CallCommandAsync(Commands command)
        {
            try
            {
                await _commandHandlerService.HandleCommandAsync(command);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unknown error while running command. {Command} {Error}", command, e);
            }
        }
    }
}