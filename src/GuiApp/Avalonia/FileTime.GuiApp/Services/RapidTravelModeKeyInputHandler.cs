using Avalonia.Input;
using FileTime.App.Core.Command;
using FileTime.App.Core.Models;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.Services;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Models;
using FileTime.Core.Services;
using FileTime.GuiApp.Configuration;
using FileTime.GuiApp.Extensions;
using FileTime.GuiApp.Models;
using Microsoft.Extensions.Logging;

namespace FileTime.GuiApp.Services;

public class RapidTravelModeKeyInputHandler : IRapidTravelModeKeyInputHandler
{
    private const string RAPIDTRAVELFILTERNAME = "rapid_travel_filter";
    private readonly IAppState _appState;
    private readonly IModalService _modalService;
    private readonly IKeyboardConfigurationService _keyboardConfigurationService;
    private readonly ICommandHandlerService _commandHandlerService;
    private readonly ILogger<RapidTravelModeKeyInputHandler> _logger;
    private readonly BindedCollection<IModalViewModelBase> _openModals;
    private ITabViewModel? _selectedTab;

    public RapidTravelModeKeyInputHandler(
        IAppState appState,
        IModalService modalService,
        IKeyboardConfigurationService keyboardConfigurationService,
        ICommandHandlerService commandHandlerService,
        ILogger<RapidTravelModeKeyInputHandler> logger)
    {
        _appState = appState;
        _modalService = modalService;
        _keyboardConfigurationService = keyboardConfigurationService;
        _commandHandlerService = commandHandlerService;
        _logger = logger;
        
        _appState.SelectedTab.Subscribe(t => _selectedTab = t);

        _openModals = new BindedCollection<IModalViewModelBase>(modalService.OpenModals);
    }

    public async Task HandleInputKey(Key key, SpecialKeysStatus specialKeysStatus, Action<bool> setHandled)
    {
        var keyString = key.ToString();
        var updateRapidTravelFilter = false;

        if (key == Key.Escape)
        {
            setHandled(true);
            if ((_openModals.Collection?.Count ?? 0) > 0)
            {
                _modalService.CloseModal(_openModals.Collection!.Last());
            }
            else
            {
                _appState.SwitchViewMode(ViewMode.Default);
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
            var currentKeyAsList = new List<KeyConfig>() {new KeyConfig(key)};
            var selectedCommandBinding = _keyboardConfigurationService.UniversalCommandBindings.FirstOrDefault(c => c.Keys.AreKeysEqual(currentKeyAsList));
            if (selectedCommandBinding != null)
            {
                setHandled(true);
                await CallCommandAsync(selectedCommandBinding.Command);
            }
        }

        if (updateRapidTravelFilter)
        {
            if (_selectedTab?.Tab is not ITab tab) return;
            
            tab.RemoveItemFilter(RAPIDTRAVELFILTERNAME);
            tab.AddItemFilter(new ItemFilter(RAPIDTRAVELFILTERNAME, i => i.Name.ToLower().Contains(_appState.RapidTravelText)));
            /*var currentLocation = await _appState.SelectedTab.CurrentLocation.Container.WithoutVirtualContainer(MainPageViewModel.RAPIDTRAVEL);
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
            }*/
        }
    }

    private async Task CallCommandAsync(Command command)
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