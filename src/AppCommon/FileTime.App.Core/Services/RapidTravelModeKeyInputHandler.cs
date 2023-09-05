using FileTime.App.Core.Configuration;
using FileTime.App.Core.Extensions;
using FileTime.App.Core.UserCommand;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using GeneralInputKey;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace FileTime.App.Core.Services;

public class RapidTravelModeKeyInputHandler : IRapidTravelModeKeyInputHandler
{
    private const string RapidTravelFilterName = "rapid_travel_filter";

    private readonly IAppState _appState;
    private readonly IModalService _modalService;
    private readonly IKeyboardConfigurationService _keyboardConfigurationService;
    private readonly IUserCommandHandlerService _userCommandHandlerService;
    private readonly ILogger<RapidTravelModeKeyInputHandler> _logger;
    private readonly IIdentifiableUserCommandService _identifiableUserCommandService;
    private ITabViewModel? _selectedTab;

    public RapidTravelModeKeyInputHandler(
        IAppState appState,
        IModalService modalService,
        IKeyboardConfigurationService keyboardConfigurationService,
        IUserCommandHandlerService userCommandHandlerService,
        ILogger<RapidTravelModeKeyInputHandler> logger,
        IIdentifiableUserCommandService identifiableUserCommandService)
    {
        _appState = appState;
        _modalService = modalService;
        _keyboardConfigurationService = keyboardConfigurationService;
        _userCommandHandlerService = userCommandHandlerService;
        _logger = logger;
        _identifiableUserCommandService = identifiableUserCommandService;

        _appState.SelectedTab.Subscribe(t => _selectedTab = t);

        _appState.RapidTravelTextDebounced.Subscribe((v, _) =>
        {
            if (_selectedTab?.Tab is not { } tab) return Task.CompletedTask;
            tab.RemoveItemFilter(RapidTravelFilterName);

            if (v is null) return Task.CompletedTask;

            tab.AddItemFilter(new ItemFilter(RapidTravelFilterName, i => i.Name.ToLower().Contains(v)));
            return Task.CompletedTask;
        });
    }

    public async Task HandleInputKey(GeneralKeyEventArgs args)
    {
        if (args.Key is not { } key) return;
        var keyString = key.Humanize();

        if (key == Keys.Escape)
        {
            args.Handled = true;
            if (_modalService.OpenModals.Count > 0)
            {
                _modalService.CloseModal(_modalService.OpenModals.Last());
            }
            else
            {
                await CallCommandAsync(ExitRapidTravelCommand.Instance);
            }
        }
        else if (key == Keys.Backspace)
        {
            if (_appState.RapidTravelText.Value!.Length > 0)
            {
                args.Handled = true;
                await _appState.SetRapidTravelTextAsync(
                    _appState.RapidTravelText.Value![..^1]
                );
            }
        }
        else if (keyString.Length == 1)
        {
            args.Handled = true;
            await _appState.SetRapidTravelTextAsync(
                _appState.RapidTravelText.Value + keyString.ToLower()
            );
        }
        else
        {
            var currentKeyAsList = new List<KeyConfig> {new(key)};
            var selectedCommandBinding = _keyboardConfigurationService.UniversalCommandBindings.FirstOrDefault(c => c.Keys.AreKeysEqual(currentKeyAsList));
            if (selectedCommandBinding != null)
            {
                args.Handled = true;
                await CallCommandAsync(_identifiableUserCommandService.GetCommand(selectedCommandBinding.Command)!);
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