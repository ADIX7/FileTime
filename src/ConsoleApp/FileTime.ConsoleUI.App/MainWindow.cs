using System.Collections.ObjectModel;
using System.Linq.Expressions;
using DeclarativeProperty;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.ViewModels;
using TerminalUI;
using TerminalUI.Color;
using TerminalUI.Controls;
using TerminalUI.Extensions;
using TerminalUI.Models;
using TerminalUI.ViewExtensions;
using ConsoleColor = TerminalUI.Color.ConsoleColor;

namespace FileTime.ConsoleUI.App;

public class MainWindow
{
    private readonly IConsoleAppState _consoleAppState;
    private readonly IApplicationContext _applicationContext;
    private readonly ITheme _theme;
    private ListView<IAppState, IItemViewModel> _selectedItemsView;

    private Grid<object> _grid;

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

        TestGrid();
    }

    private void TestGrid()
    {
        var grid = new Grid<object>
        {
            ApplicationContext = _applicationContext,
            ColumnDefinitionsObject = "Auto Auto",
            RowDefinitionsObject = "Auto Auto",
            ChildInitializer =
            {
                new Rectangle<object>
                {
                    Fill = new ConsoleColor(System.ConsoleColor.Blue, ColorType.Foreground),
                    Extensions =
                    {
                        new GridPositionExtension(0, 0)
                    },
                    Width = 2,
                    Height = 2,
                },
                new Rectangle<object>
                {
                    Fill = new ConsoleColor(System.ConsoleColor.Red, ColorType.Foreground),
                    Extensions =
                    {
                        new GridPositionExtension(0, 1)
                    },
                    Width = 3,
                    Height = 3,
                },
                new Rectangle<object>
                {
                    Fill = new ConsoleColor(System.ConsoleColor.Green, ColorType.Foreground),
                    Extensions =
                    {
                        new GridPositionExtension(1, 0)
                    },
                    Width = 4,
                    Height = 4,
                },
                new Rectangle<object>
                {
                    Fill = new ConsoleColor(System.ConsoleColor.Yellow, ColorType.Foreground),
                    Extensions =
                    {
                        new GridPositionExtension(1, 1)
                    },
                    Width = 5,
                    Height = 5,
                }
            }
        };

        _grid = grid;
    }

    public IEnumerable<IView> RootViews() => new IView[]
    {
        _grid, _selectedItemsView
    };

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