using Avalonia.Input;
using FileTime.GuiApp.Models;

namespace FileTime.GuiApp.Services;

public class RapidTravelModeKeyInputHandler : IRapidTravelModeKeyInputHandler
{
    public async Task HandleInputKey(Key key, SpecialKeysStatus specialKeysStatus, Action<bool> setHandled)
    {
        /*var keyString = key.ToString();
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
        }*/
    }
}