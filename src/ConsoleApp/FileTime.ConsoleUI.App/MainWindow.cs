using DeclarativeProperty;
using FileTime.App.Core.ViewModels;
using FileTime.ConsoleUI.App.Controls;
using FileTime.Core.Models;
using Terminal.Gui;

namespace FileTime.ConsoleUI.App;

public class MainWindow
{
    private readonly IConsoleAppState _consoleAppState;
    private readonly ListView _selectedItemsView;

    public MainWindow(IConsoleAppState consoleAppState)
    {
        _consoleAppState = consoleAppState;

        _selectedItemsView = new() {X = 1, Y = 0, Width = Dim.Fill(), Height = Dim.Fill()};
        _selectedItemsView.AddKeyBinding(Key.Space, Command.ToggleChecked);
        _selectedItemsView.OpenSelectedItem += (e) =>
        {
            if (e.Value is IItemViewModel {BaseItem: IContainer container}
                && consoleAppState.SelectedTab.Value?.Tab is { } tab)
                tab.SetCurrentLocation(container);
        };
    }

    public void Initialize()
    {
        var selectedsItems = _consoleAppState
            .SelectedTab
            .Map(t => t.CurrentItems)
            .Switch();

        var renderer = new ItemRenderer(selectedsItems, _selectedItemsView);
        _selectedItemsView.Source = renderer;
    }

    public IEnumerable<View> GetElements()
    {
        return new View[] {_selectedItemsView};
    }
}