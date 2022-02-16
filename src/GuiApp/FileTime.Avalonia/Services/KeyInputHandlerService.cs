using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using FileTime.App.Core.Command;
using FileTime.Avalonia.Application;
using FileTime.Avalonia.Configuration;
using FileTime.Avalonia.ViewModels;
using FileTime.Core.Extensions;
using FileTime.Core.Models;
using Microsoft.Extensions.Logging;

namespace FileTime.Avalonia.Services
{
    public class KeyInputHandlerService
    {
        private readonly List<KeyConfig[]> _keysToSkip = new();
        private readonly AppState _appState;
        private readonly KeyboardConfigurationService _keyboardConfigurationService;
        private readonly CommandHandlerService _commandHandlerService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<KeyInputHandlerService> _logger;

        public KeyInputHandlerService(
            AppState appState,
            KeyboardConfigurationService keyboardConfigurationService,
            CommandHandlerService commandHandlerService,
            IDialogService dialogService,
            ILogger<KeyInputHandlerService> logger)
        {
            _appState = appState;
            _keyboardConfigurationService = keyboardConfigurationService;
            _commandHandlerService = commandHandlerService;
            _dialogService = dialogService;
            _logger = logger;

            _keysToSkip.Add(new KeyConfig[] { new KeyConfig(Key.Up) });
            _keysToSkip.Add(new KeyConfig[] { new KeyConfig(Key.Down) });
            _keysToSkip.Add(new KeyConfig[] { new KeyConfig(Key.Tab) });
            _keysToSkip.Add(new KeyConfig[] { new KeyConfig(Key.PageDown) });
            _keysToSkip.Add(new KeyConfig[] { new KeyConfig(Key.PageUp) });
            _keysToSkip.Add(new KeyConfig[] { new KeyConfig(Key.F4, alt: true) });
            _keysToSkip.Add(new KeyConfig[] { new KeyConfig(Key.LWin) });
            _keysToSkip.Add(new KeyConfig[] { new KeyConfig(Key.RWin) });
        }
        public async Task ProcessKeyDown(Key key, KeyModifiers keyModifiers, Action<bool> setHandled)
        {
            if (key == Key.LeftAlt
                || key == Key.RightAlt
                || key == Key.LeftShift
                || key == Key.RightShift
                || key == Key.LeftCtrl
                || key == Key.RightCtrl) return;

            _appState.NoCommandFound = false;

            var isAltPressed = (keyModifiers & KeyModifiers.Alt) == KeyModifiers.Alt;
            var isShiftPressed = (keyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift;
            var isCtrlPressed = (keyModifiers & KeyModifiers.Control) == KeyModifiers.Control;

            if (_appState.ViewMode == ViewMode.Default)
            {
                var keyWithModifiers = new KeyConfig(key, shift: isShiftPressed, alt: isAltPressed, ctrl: isCtrlPressed);
                _appState.PreviousKeys.Add(keyWithModifiers);

                var selectedCommandBinding = _keyboardConfigurationService.UniversalCommandBindings.FirstOrDefault(c => AreKeysEqual(c.Keys, _appState.PreviousKeys));
                selectedCommandBinding ??= _keyboardConfigurationService.CommandBindings.FirstOrDefault(c => AreKeysEqual(c.Keys, _appState.PreviousKeys));

                if (key == Key.Escape)
                {
                    _appState.IsAllShortcutVisible = false;
                    _appState.MessageBoxText = null;
                    _appState.PreviousKeys.Clear();
                    _appState.PossibleCommands = new();
                    setHandled(true);
                }
                else if (key == Key.Enter
                    && _appState.MessageBoxText != null)
                {
                    _appState.PreviousKeys.Clear();
                    _dialogService.ProcessMessageBox();
                    setHandled(true);
                }
                else if (selectedCommandBinding != null)
                {
                    setHandled(true);
                    _appState.PreviousKeys.Clear();
                    _appState.PossibleCommands = new();
                    await CallCommandAsync(selectedCommandBinding.Command);
                }
                else if (_keysToSkip.Any(k => AreKeysEqual(k, _appState.PreviousKeys)))
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
                    var possibleCommands = _keyboardConfigurationService.AllShortcut.Where(c => AreKeysEqual(c.Keys[0], keyWithModifiers)).ToList();

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
            else
            {
                var keyString = key.ToString();
                var updateRapidTravelFilter = false;

                if (key == Key.Escape)
                {
                    setHandled(true);
                    if (_appState.IsAllShortcutVisible)
                    {
                        _appState.IsAllShortcutVisible = false;
                    }
                    else if (_appState.MessageBoxText != null)
                    {
                        _appState.MessageBoxText = null;
                    }
                    else
                    {
                        await _appState.ExitRapidTravelMode();
                    }
                }
                else if (key == Key.Back)
                {
                    if (_appState.RapidTravelText.Length > 0)
                    {
                        setHandled(true);
                        _appState.RapidTravelText = _appState.RapidTravelText.Substring(0, _appState.RapidTravelText.Length - 1);
                        updateRapidTravelFilter = true;
                    }
                }
                else if (keyString.Length == 1)
                {
                    setHandled(true);
                    _appState.RapidTravelText += keyString.ToLower();
                    updateRapidTravelFilter = true;
                }
                else
                {
                    var currentKeyAsList = new List<KeyConfig>() { new KeyConfig(key) };
                    var selectedCommandBinding = _keyboardConfigurationService.UniversalCommandBindings.FirstOrDefault(c => AreKeysEqual(c.Keys, currentKeyAsList));
                    if (selectedCommandBinding != null)
                    {
                        setHandled(true);
                        await CallCommandAsync(selectedCommandBinding.Command);
                    }
                }

                if (updateRapidTravelFilter)
                {
                    var currentLocation = await _appState.SelectedTab.CurrentLocation.Container.WithoutVirtualContainer(MainPageViewModel.RAPIDTRAVEL);
                    var newLocation = new VirtualContainer(
                        currentLocation,
                        new List<Func<IEnumerable<IContainer>, IEnumerable<IContainer>>>()
                        {
                            container => container.Where(c => c.Name.ToLower().Contains(_appState.RapidTravelText))
                        },
                        new List<Func<IEnumerable<IElement>, IEnumerable<IElement>>>()
                        {
                            element => element.Where(e => e.Name.ToLower().Contains(_appState.RapidTravelText))
                        },
                        virtualContainerName: MainPageViewModel.RAPIDTRAVEL
                    );

                    await newLocation.Init();

                    await _appState.SelectedTab.OpenContainer(newLocation);

                    var selectedItemName = _appState.SelectedTab.SelectedItem?.Item.Name;
                    var currentLocationItems = await _appState.SelectedTab.CurrentLocation.GetItems();
                    if (currentLocationItems.FirstOrDefault(i => string.Equals(i.Item.Name, _appState.RapidTravelText, StringComparison.OrdinalIgnoreCase)) is IItemViewModel matchItem)
                    {
                        await _appState.SelectedTab.SetCurrentSelectedItem(matchItem.Item);
                    }
                    else if (!currentLocationItems.Select(i => i.Item.Name).Any(n => n == selectedItemName))
                    {
                        await _appState.SelectedTab.MoveCursorToFirst();
                    }
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

        private static bool AreKeysEqual(IReadOnlyList<KeyConfig> collection1, IReadOnlyList<KeyConfig> collection2)
        {
            if (collection1.Count != collection2.Count) return false;

            for (var i = 0; i < collection1.Count; i++)
            {
                if (!AreKeysEqual(collection1[i], collection2[i])) return false;
            }

            return true;
        }

        private static bool AreKeysEqual(KeyConfig key1, KeyConfig key2) =>
            key1.Key == key2.Key
            && key1.Alt == key2.Alt
            && key1.Shift == key2.Shift
            && key1.Ctrl == key2.Ctrl;
    }
}