using System.Collections.ObjectModel;
using DeclarativeProperty;
using FileTime.App.Core.Configuration;
using FileTime.App.Core.Extensions;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.Core.Models.Extensions;
using GeneralInputKey;
using Microsoft.Extensions.Logging;

namespace FileTime.App.Core.Services;

public class DefaultModeKeyInputHandler : IDefaultModeKeyInputHandler
{
    private readonly IAppState _appState;
    private readonly IModalService _modalService;
    private readonly IKeyboardConfigurationService _keyboardConfigurationService;
    private readonly List<KeyConfig[]> _keysToSkip = new();
    private readonly IDeclarativeProperty<IContainer?> _currentLocation;
    private readonly ILogger<DefaultModeKeyInputHandler> _logger;
    private readonly IUserCommandHandlerService _userCommandHandlerService;
    private readonly IIdentifiableUserCommandService _identifiableUserCommandService;
    private readonly IPossibleCommandsService _possibleCommandsService;
    private readonly ReadOnlyObservableCollection<IModalViewModel> _openModals;

    public DefaultModeKeyInputHandler(
        IAppState appState,
        IModalService modalService,
        IKeyboardConfigurationService keyboardConfigurationService,
        ILogger<DefaultModeKeyInputHandler> logger,
        IUserCommandHandlerService userCommandHandlerService,
        IIdentifiableUserCommandService identifiableUserCommandService,
        IPossibleCommandsService possibleCommandsService)
    {
        _appState = appState;
        _identifiableUserCommandService = identifiableUserCommandService;
        _possibleCommandsService = possibleCommandsService;
        _keyboardConfigurationService = keyboardConfigurationService;
        _logger = logger;
        _modalService = modalService;
        _userCommandHandlerService = userCommandHandlerService;

        _currentLocation = _appState.SelectedTab
            .Map(t => t?.CurrentLocation)
            .Switch();

        _openModals = modalService.OpenModals;

        _keysToSkip.Add(new[] {new KeyConfig(Keys.Up)});
        _keysToSkip.Add(new[] {new KeyConfig(Keys.Down)});
        _keysToSkip.Add(new[] {new KeyConfig(Keys.Tab)});
        _keysToSkip.Add(new[] {new KeyConfig(Keys.PageDown)});
        _keysToSkip.Add(new[] {new KeyConfig(Keys.PageUp)});
        _keysToSkip.Add(new[] {new KeyConfig(Keys.F4, alt: true)});
        _keysToSkip.Add(new[] {new KeyConfig(Keys.LWin)});
        _keysToSkip.Add(new[] {new KeyConfig(Keys.RWin)});
    }

    public async Task HandleInputKey(GeneralKeyEventArgs args)
    {
        if (args.Key is not { } key) return;
        var keyWithModifiers = new KeyConfig(
            key, 
            shift: args.SpecialKeysStatus.IsShiftPressed, 
            alt: args.SpecialKeysStatus.IsAltPressed, 
            ctrl: args.SpecialKeysStatus.IsCtrlPressed);
        
        _appState.PreviousKeys.Add(keyWithModifiers);

        var selectedCommandBinding = _keyboardConfigurationService.UniversalCommandBindings.FirstOrDefault(c => c.Keys.AreKeysEqual(_appState.PreviousKeys));
        selectedCommandBinding ??= _keyboardConfigurationService.CommandBindings.FirstOrDefault(c => c.Keys.AreKeysEqual(_appState.PreviousKeys));

        if (key == Keys.Escape)
        {
            var doGeneralReset = _appState.PreviousKeys.Count > 1;

            if (_openModals.Count > 0)
            {
                _modalService.CloseModal(_openModals.Last());
            }
            else if (_appState.RapidTravelText.Value != "")
            {
                await _appState.SetRapidTravelTextAsync("");
            }
            else if (_currentLocation.Value?.GetExtension<EscHandlerContainerExtension>() is { } escHandler)
            {
                var escapeResult = await escHandler.HandleEsc();
                if (escapeResult.NavigateTo != null)
                {
                    args.Handled = true;
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
                args.Handled = true;
                _appState.PreviousKeys.Clear();
                _possibleCommandsService.Clear();
            }
        }
        /*else if (key == Key.Enter
                 && _appState.MessageBoxText != null)
        {
            _appState.PreviousKeys.Clear();
            //_dialogService.ProcessMessageBox();
            args.Handled = true;
        }*/
        else if (selectedCommandBinding != null)
        {
            args.Handled = true;
            _appState.PreviousKeys.Clear();
            _possibleCommandsService.Clear();
            var command = _identifiableUserCommandService.GetCommand(selectedCommandBinding.Command);
            if (command is not null)
            {
                await CallCommandAsync(command);
            }
        }
        else if (_keysToSkip.Any(k => k.AreKeysEqual(_appState.PreviousKeys)))
        {
            _appState.PreviousKeys.Clear();
            _possibleCommandsService.Clear();
            return;
        }
        else if (_appState.PreviousKeys.Count == 2)
        {
            args.Handled = true;
            _appState.NoCommandFound = true;
            _appState.PreviousKeys.Clear();
            _possibleCommandsService.Clear();
        }
        else
        {
            args.Handled = true;
            var possibleCommands = _keyboardConfigurationService.AllShortcut.Where(c => c.Keys[0].AreEquals(keyWithModifiers)).ToList();

            if (possibleCommands.Count == 0)
            {
                _appState.NoCommandFound = true;
                _appState.PreviousKeys.Clear();
            }
            else
            {
                _possibleCommandsService.Clear();
                _possibleCommandsService.AddRange(possibleCommands);
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