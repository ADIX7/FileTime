using System.Collections.ObjectModel;
using System.Linq.Expressions;
using DeclarativeProperty;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.ViewModels;
using TerminalUI;
using TerminalUI.Controls;
using TerminalUI.Extensions;
using TerminalUI.Models;

namespace FileTime.ConsoleUI.App;

public class MainWindow
{
    private readonly IConsoleAppState _consoleAppState;
    private readonly IApplicationContext _applicationContext;
    private readonly ITheme _theme;
    private ListView<IAppState, IItemViewModel> _selectedItemsView;

    public MainWindow(
        IConsoleAppState consoleAppState,
        IApplicationContext applicationContext,
        ITheme theme)
    {
        _consoleAppState = consoleAppState;
        _applicationContext = applicationContext;
        _theme = theme;
    }

    public void Initialize()
    {
        _selectedItemsView = new()
        {
            DataContext = _consoleAppState,
            ApplicationContext = _applicationContext
        };

        _selectedItemsView.ItemTemplate = item =>
        {
            var textBlock = item.CreateChild<TextBlock<IItemViewModel>>();
            textBlock.Bind(
                textBlock,
                dc => dc == null ? string.Empty : dc.DisplayNameText,
                tb => tb.Text
            );
            textBlock.Bind(
                textBlock,
                dc => dc == null ? _theme.DefaultForegroundColor : ToForegroundColor(dc.ViewMode.Value),
                tb => tb.Foreground
            );

            return textBlock;
        };

        _selectedItemsView.Bind(
            _selectedItemsView,
            appState => appState == null ? null : appState.SelectedTab.Map(t => t == null ? null : t.CurrentItems).Switch(),
            v => v.ItemsSource);
    }

    public IEnumerable<IView> RootViews() => new IView[] {_selectedItemsView};

    private IColor? ToForegroundColor(ItemViewMode viewMode)
        => viewMode switch
        {
            ItemViewMode.Default => _theme.DefaultForegroundColor,
            ItemViewMode.Alternative => _theme.AlternativeItemForegroundColor,
            ItemViewMode.Selected => _theme.SelectedItemForegroundColor,
            ItemViewMode.Marked => _theme.MarkedItemForegroundColor,
            ItemViewMode.MarkedSelected => _theme.MarkedSelectedItemForegroundColor,
            ItemViewMode.MarkedAlternative => _theme.MarkedAlternativeItemForegroundColor,
            _ => throw new NotImplementedException()
        };

    private IColor? ToBackgroundColor(ItemViewMode viewMode)
        => viewMode switch
        {
            ItemViewMode.Default => _theme.DefaultBackgroundColor,
            ItemViewMode.Alternative => _theme.AlternativeItemBackgroundColor,
            ItemViewMode.Selected => _theme.SelectedItemBackgroundColor,
            ItemViewMode.Marked => _theme.MarkedItemBackgroundColor,
            ItemViewMode.MarkedSelected => _theme.MarkedSelectedItemBackgroundColor,
            ItemViewMode.MarkedAlternative => _theme.MarkedAlternativeItemBackgroundColor,
            _ => throw new NotImplementedException()
        };
}