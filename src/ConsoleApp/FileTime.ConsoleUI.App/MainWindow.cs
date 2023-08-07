using System.Collections.ObjectModel;
using DeclarativeProperty;
using FileTime.App.Core.ViewModels;
using FileTime.ConsoleUI.App.Extensions;
using TerminalUI;
using TerminalUI.Controls;
using TerminalUI.Extensions;

namespace FileTime.ConsoleUI.App;

public class MainWindow
{
    private readonly IConsoleAppState _consoleAppState;
    private const int ParentColumnWidth = 20;

    public MainWindow(IConsoleAppState consoleAppState)
    {
        _consoleAppState = consoleAppState;
    }

    public void Initialize()
    {
        ListView<IAppState, IItemViewModel> selectedItemsView = new();
        selectedItemsView.DataContext = _consoleAppState;

        selectedItemsView.Bind(
            selectedItemsView,
            appState => appState.SelectedTab.Map(t => t == null ? null : t.CurrentItems).Switch(),
            v => v.ItemsSource);
        
        selectedItemsView.Render();
    }
}