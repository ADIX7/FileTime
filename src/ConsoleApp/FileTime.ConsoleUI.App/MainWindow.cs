using System.Linq.Expressions;
using DeclarativeProperty;
using FileTime.App.Core.ViewModels;
using TerminalUI;
using TerminalUI.Controls;
using TerminalUI.Extensions;

namespace FileTime.ConsoleUI.App;

public class MainWindow
{
    private readonly IConsoleAppState _consoleAppState;
    private readonly IApplicationContext _applicationContext;
    private const int ParentColumnWidth = 20;

    public MainWindow(IConsoleAppState consoleAppState, IApplicationContext applicationContext)
    {
        _consoleAppState = consoleAppState;
        _applicationContext = applicationContext;
    }

    public void Initialize()
    {
        ListView<IAppState, IItemViewModel> selectedItemsView = new()
        {
            ApplicationContext = _applicationContext
        };
        selectedItemsView.DataContext = _consoleAppState;
        selectedItemsView.ItemTemplate = item =>
        {
            var textBlock = item.CreateChild<TextBlock<IItemViewModel>>();
            textBlock.Bind(
                textBlock,
                dc => dc == null ? string.Empty : dc.DisplayNameText,
                tb => tb.Text
            );

            return textBlock;
        };

        selectedItemsView.Bind(
            selectedItemsView,
            appState => appState.SelectedTab.Map(t => t == null ? null : t.CurrentItems).Switch(),
            v => v.ItemsSource);
        
        selectedItemsView.RequestRerender();
    }
}