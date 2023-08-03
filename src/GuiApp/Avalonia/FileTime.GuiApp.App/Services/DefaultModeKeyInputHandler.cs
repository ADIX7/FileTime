using Avalonia.Input;
using FileTime.App.Core.Services;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Extensions;
using FileTime.Core.Models;
using FileTime.Core.Models.Extensions;
using FileTime.GuiApp.App.Configuration;
using FileTime.GuiApp.App.Extensions;
using FileTime.GuiApp.App.Models;
using FileTime.GuiApp.App.ViewModels;
using Microsoft.Extensions.Logging;
using DeclarativeProperty;

namespace FileTime.GuiApp.App.Services;

public class DefaultModeKeyInputHandler : IDefaultModeKeyInputHandler
{
    private readonly IGuiAppState _appState;
    private readonly IModalService _modalService;
    private readonly IKeyboardConfigurationService _keyboardConfigurationService;
    private readonly List<KeyConfig[]> _keysToSkip = new();
    private readonly IDeclarativeProperty<IContainer?> _currentLocation;
    private readonly ILogger<DefaultModeKeyInputHandler> _logger;
    private readonly IUserCommandHandlerService _userCommandHandlerService;
    private readonly IIdentifiableUserCommandService _identifiableUserCommandService;
    private readonly BindedCollection<IModalViewModel> _openModals;

    public DefaultModeKeyInputHandler(
        IGuiAppState appState,
        IModalService modalService,
        IKeyboardConfigurationService keyboardConfigurationService,
        ILogger<DefaultModeKeyInputHandler> logger,
        IUserCommandHandlerService userCommandHandlerService,
        IIdentifiableUserCommandService identifiableUserCommandService)
    {
        _appState = appState;
        _identifiableUserCommandService = identifiableUserCommandService;
        _keyboardConfigurationService = keyboardConfigurationService;
        _logger = logger;
        _modalService = modalService;
        _userCommandHandlerService = userCommandHandlerService;

        _currentLocation = _appState.SelectedTab
            .Map(t => t?.CurrentLocation)
            .Switch();

        _openModals = modalService.OpenModals.ToBindedCollection();

        _keysToSkip.Add(new[] {new KeyConfig(Key.Up)});
        _keysToSkip.Add(new[] {new KeyConfig(Key.Down)});
        _keysToSkip.Add(new[] {new KeyConfig(Key.Tab)});
        _keysToSkip.Add(new[] {new KeyConfig(Key.PageDown)});
        _keysToSkip.Add(new[] {new KeyConfig(Key.PageUp)});
        _keysToSkip.Add(new[] {new KeyConfig(Key.F4, alt: true)});
        _keysToSkip.Add(new[] {new KeyConfig(Key.LWin)});
        _keysToSkip.Add(new[] {new KeyConfig(Key.RWin)});
    }

    public async Task HandleInputKey(Key key, SpecialKeysStatus specialKeysStatus, Action<bool> setHandled)
    {
        var keyWithModifiers = new KeyConfig(key, shift: specialKeysStatus.IsShiftPressed, alt: specialKeysStatus.IsAltPressed, ctrl: specialKeysStatus.IsCtrlPressed);
        _appState.PreviousKeys.Add(keyWithModifiers);

        var selectedCommandBinding = _keyboardConfigurationService.UniversalCommandBindings.FirstOrDefault(c => c.Keys.AreKeysEqual(_appState.PreviousKeys));
        selectedCommandBinding ??= _keyboardConfigurationService.CommandBindings.FirstOrDefault(c => c.Keys.AreKeysEqual(_appState.PreviousKeys));

        if (key == Key.Escape)
        {
            var doGeneralReset = _appState.PreviousKeys.Count > 1 || _appState.IsAllShortcutVisible;

            if ((_openModals.Collection?.Count ?? 0) > 0)
            {
                _modalService.CloseModal(_openModals.Collection!.Last());
            }
            else if (_currentLocation.Value?.GetExtension<EscHandlerContainerExtension>() is { } escHandler)
            {
                var escapeResult = await escHandler.HandleEsc();
                if (escapeResult.NavigateTo != null)
                {
                    setHandled(true);
                    _appState.PreviousKeys.Clear();
                    if (_appState.SelectedTab.Value?.Tab is { } selectedTab)
                    {
                        await selectedTab.SetCurrentLocation(escapeResult.NavigateTo);
                    }
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
            }

            if (doGeneralReset)
            {
                setHandled(true);
                _appState.IsAllShortcutVisible = false;
                _appState.PreviousKeys.Clear();
                _appState.PossibleCommands = new();
            }
        }
        /*else if (key == Key.Enter
                 && _appState.MessageBoxText != null)
        {
            _appState.PreviousKeys.Clear();
            //_dialogService.ProcessMessageBox();
            setHandled(true);
        }*/
        else if (selectedCommandBinding != null)
        {
            setHandled(true);
            _appState.PreviousKeys.Clear();
            _appState.PossibleCommands = new();
            var command = _identifiableUserCommandService.GetCommand(selectedCommandBinding.Command);
            if (command is not null)
            {
                await CallCommandAsync(command);
            }
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

    private async Task CallCommandAsync(IUserCommand command)
    {
        try
        {
            await _userCommandHandlerService.HandleCommandAsync(command);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unknown error while running command. {Command} {Error}", command.GetType().Name, e);
        }
    }
}