using Avalonia.Input;
using FileTime.App.Core.Configuration;
using FileTime.App.Core.Extensions;
using FileTime.App.Core.Models;
using FileTime.App.Core.Services;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Extensions;
using FileTime.Core.Models;
using FileTime.GuiApp.App.Models;
using Microsoft.Extensions.Logging;

namespace FileTime.GuiApp.App.Services;

public class RapidTravelModeKeyInputHandler : IRapidTravelModeKeyInputHandler
{
    private const string RapidTravelFilterName = "rapid_travel_filter";

    private readonly IAppState _appState;
    private readonly IModalService _modalService;
    private readonly IKeyboardConfigurationService _keyboardConfigurationService;
    private readonly IUserCommandHandlerService _userCommandHandlerService;
    private readonly ILogger<RapidTravelModeKeyInputHandler> _logger;
    private readonly IIdentifiableUserCommandService _identifiableUserCommandService;
    private readonly IAppKeyService<Key> _appKeyService;
    private readonly BindedCollection<IModalViewModel> _openModals;
    private ITabViewModel? _selectedTab;

    public RapidTravelModeKeyInputHandler(
        IAppState appState,
        IModalService modalService,
        IKeyboardConfigurationService keyboardConfigurationService,
        IUserCommandHandlerService userCommandHandlerService,
        ILogger<RapidTravelModeKeyInputHandler> logger,
        IIdentifiableUserCommandService identifiableUserCommandService,
        IAppKeyService<Key> appKeyService)
    {
        _appState = appState;
        _modalService = modalService;
        _keyboardConfigurationService = keyboardConfigurationService;
        _userCommandHandlerService = userCommandHandlerService;
        _logger = logger;
        _identifiableUserCommandService = identifiableUserCommandService;
        _appKeyService = appKeyService;

        _appState.SelectedTab.Subscribe(t => _selectedTab = t);

        _openModals = modalService.OpenModals.ToBindedCollection();

        _appState.RapidTravelTextDebounced.Subscribe((v, _) =>
        {
            if (_selectedTab?.Tab is not { } tab) return Task.CompletedTask;
            tab.RemoveItemFilter(RapidTravelFilterName);

            if (v is null) return Task.CompletedTask;

            tab.AddItemFilter(new ItemFilter(RapidTravelFilterName, i => i.Name.ToLower().Contains(v)));
            return Task.CompletedTask;
        });
    }

    public async Task HandleInputKey(Key key2, SpecialKeysStatus specialKeysStatus, Action<bool> setHandled)
    {
        if (_appKeyService.MapKey(key2) is not { } key) return;
        var keyString = key.ToString();

        if (key == Keys.Escape)
        {
            setHandled(true);
            if ((_openModals.Collection?.Count ?? 0) > 0)
            {
                _modalService.CloseModal(_openModals.Collection!.Last());
            }
            else
            {
                await CallCommandAsync(ExitRapidTravelCommand.Instance);
            }
        }
        else if (key == Keys.Back)
        {
            if (_appState.RapidTravelText.Value!.Length > 0)
            {
                setHandled(true);
                await _appState.RapidTravelText.SetValue(
                    _appState.RapidTravelText.Value![..^1]
                );
            }
        }
        else if (keyString.Length == 1)
        {
            setHandled(true);
            await _appState.RapidTravelText.SetValue(
                _appState.RapidTravelText.Value + keyString.ToLower()
            );
        }
        else
        {
            var currentKeyAsList = new List<KeyConfig> {new(key)};
            var selectedCommandBinding = _keyboardConfigurationService.UniversalCommandBindings.FirstOrDefault(c => c.Keys.AreKeysEqual(currentKeyAsList));
            if (selectedCommandBinding != null)
            {
                setHandled(true);
                await CallCommandAsync(_identifiableUserCommandService.GetCommand(selectedCommandBinding.Command));
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