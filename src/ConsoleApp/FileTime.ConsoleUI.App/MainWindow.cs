using DeclarativeProperty;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.ViewModels;
using FileTime.Core.Enums;
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

    private IView _root;

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
        var root = new Grid<IAppState>
        {
            DataContext = _consoleAppState,
            ApplicationContext = _applicationContext,
            RowDefinitionsObject = "Auto *",
            ChildInitializer =
            {
                new Grid<IAppState>
                {
                    ColumnDefinitionsObject = "* Auto",
                    ChildInitializer =
                    {
                        new TextBlock<IAppState>()
                            .Setup(t =>
                                t.Bind(
                                    t,
                                    appState => appState.SelectedTab.Value.CurrentLocation.Value.FullName.Path,
                                    tb => tb.Text,
                                    value => value
                                )
                            ),
                        TabControl()
                    }
                },
                new Grid<IAppState>
                {
                    ColumnDefinitionsObject = "* 4* 4*",
                    ChildInitializer =
                    {
                        ParentsItemsView(),
                        SelectedItemsView(),
                        SelectedsItemsView(),
                    },
                    Extensions =
                    {
                        new GridPositionExtension(0, 1)
                    }
                }
            }
        };
        _root = root;
    }

    private IView<IAppState> TabControl()
    {
        var tabList = new ListView<IAppState, ITabViewModel>
        {
            Orientation = Orientation.Horizontal,
            Extensions =
            {
                new GridPositionExtension(1, 0)
            },
            ItemTemplate = item =>
            {
                var textBlock = item.CreateChild<TextBlock<ITabViewModel>>();
                textBlock.Foreground = _theme.DefaultForegroundColor;
                
                textBlock.Bind(
                    textBlock,
                    dc => dc.TabNumber.ToString(),
                    tb => tb.Text,
                    fallbackValue: "?");
                
                textBlock.Bind(
                    textBlock,
                    dc => dc.IsSelected.Value ? _theme.SelectedTabBackgroundColor : null,
                    tb => tb.Background,
                    fallbackValue: null
                );
                return textBlock;
            }
        };

        tabList.Bind(
            tabList,
            appState => appState == null ? null : appState.Tabs,
            v => v.ItemsSource);

        return tabList;
    }

    private ListView<IAppState, IItemViewModel> SelectedItemsView()
    {
        var list = new ListView<IAppState, IItemViewModel>
        {
            DataContext = _consoleAppState,
            ApplicationContext = _applicationContext,
            ListPadding = 8,
            Extensions =
            {
                new GridPositionExtension(1, 0)
            }
        };

        list.ItemTemplate = item =>
        {
            var textBlock = item.CreateChild<TextBlock<IItemViewModel>>();
            textBlock.Bind(
                textBlock,
                dc => dc == null ? string.Empty : dc.DisplayNameText,
                tb => tb.Text
            );
            textBlock.Bind(
                textBlock,
                dc => dc == null ? _theme.DefaultForegroundColor : ToForegroundColor(dc.ViewMode.Value, dc.BaseItem.Type),
                tb => tb.Foreground
            );
            textBlock.Bind(
                textBlock,
                dc => dc == null ? _theme.DefaultBackgroundColor : ToBackgroundColor(dc.ViewMode.Value, dc.BaseItem.Type),
                tb => tb.Background
            );

            return textBlock;
        };

        list.Bind(
            list,
            appState => appState == null ? null : appState.SelectedTab.Map(t => t == null ? null : t.CurrentItems).Switch(),
            v => v.ItemsSource);

        list.Bind(
            list,
            appState =>
                appState == null
                    ? null
                    : appState.SelectedTab.Value == null
                        ? null
                        : appState.SelectedTab.Value.CurrentSelectedItem.Value,
            v => v.SelectedItem);

        return list;
    }

    private ListView<IAppState, IItemViewModel> SelectedsItemsView()
    {
        var list = new ListView<IAppState, IItemViewModel>
        {
            DataContext = _consoleAppState,
            ApplicationContext = _applicationContext,
            ListPadding = 8,
            Extensions =
            {
                new GridPositionExtension(2, 0)
            }
        };

        list.ItemTemplate = item =>
        {
            var textBlock = item.CreateChild<TextBlock<IItemViewModel>>();
            textBlock.Bind(
                textBlock,
                dc => dc == null ? string.Empty : dc.DisplayNameText,
                tb => tb.Text
            );
            textBlock.Bind(
                textBlock,
                dc => dc == null ? _theme.DefaultForegroundColor : ToForegroundColor(dc.ViewMode.Value, dc.BaseItem.Type),
                tb => tb.Foreground
            );
            textBlock.Bind(
                textBlock,
                dc => dc == null ? _theme.DefaultBackgroundColor : ToBackgroundColor(dc.ViewMode.Value, dc.BaseItem.Type),
                tb => tb.Background
            );

            return textBlock;
        };

        list.Bind(
            list,
            appState => appState == null ? null : appState.SelectedTab.Map(t => t == null ? null : t.SelectedsChildren).Switch(),
            v => v.ItemsSource);

        return list;
    }

    private ListView<IAppState, IItemViewModel> ParentsItemsView()
    {
        var list = new ListView<IAppState, IItemViewModel>
        {
            DataContext = _consoleAppState,
            ApplicationContext = _applicationContext,
            ListPadding = 8,
            Extensions =
            {
                new GridPositionExtension(0, 0)
            }
        };

        list.ItemTemplate = item =>
        {
            var textBlock = item.CreateChild<TextBlock<IItemViewModel>>();
            textBlock.Bind(
                textBlock,
                dc => dc == null ? string.Empty : dc.DisplayNameText,
                tb => tb.Text
            );
            textBlock.Bind(
                textBlock,
                dc => dc == null ? _theme.DefaultForegroundColor : ToForegroundColor(dc.ViewMode.Value, dc.BaseItem.Type),
                tb => tb.Foreground
            );
            textBlock.Bind(
                textBlock,
                dc => dc == null ? _theme.DefaultBackgroundColor : ToBackgroundColor(dc.ViewMode.Value, dc.BaseItem.Type),
                tb => tb.Background
            );

            return textBlock;
        };

        list.Bind(
            list,
            appState => appState == null ? null : appState.SelectedTab.Map(t => t == null ? null : t.ParentsChildren).Switch(),
            v => v.ItemsSource);

        return list;
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

        //_grid = grid;
    }

    public IEnumerable<IView> RootViews() => new IView[]
    {
        _root
    };

    private IColor? ToForegroundColor(ItemViewMode viewMode, AbsolutePathType absolutePathType) =>
        (viewMode, absolutePathType) switch
        {
            (ItemViewMode.Default, AbsolutePathType.Container) => _theme.ContainerColor,
            (ItemViewMode.Alternative, AbsolutePathType.Container) => _theme.ContainerColor,
            (ItemViewMode.Default, _) => _theme.ElementColor,
            (ItemViewMode.Alternative, _) => _theme.ElementColor,
            (ItemViewMode.Selected, _) => ToBackgroundColor(ItemViewMode.Default, absolutePathType)?.AsForeground(),
            (ItemViewMode.Marked, _) => _theme.MarkedItemColor,
            (ItemViewMode.MarkedSelected, _) => ToBackgroundColor(ItemViewMode.Marked, absolutePathType)?.AsForeground(),
            (ItemViewMode.MarkedAlternative, _) => _theme.MarkedItemColor,
            _ => throw new NotImplementedException()
        };

    private IColor? ToBackgroundColor(ItemViewMode viewMode, AbsolutePathType absolutePathType)
        => (viewMode, absolutePathType) switch
        {
            (ItemViewMode.Default, _) => _theme.DefaultBackgroundColor,
            (ItemViewMode.Alternative, _) => _theme.DefaultBackgroundColor,
            (ItemViewMode.Selected, _) => ToForegroundColor(ItemViewMode.Default, absolutePathType)?.AsBackground(),
            (ItemViewMode.Marked, _) => _theme.MarkedItemColor,
            (ItemViewMode.MarkedSelected, _) => ToForegroundColor(ItemViewMode.Marked, absolutePathType)?.AsBackground(),
            (ItemViewMode.MarkedAlternative, _) => _theme.MarkedItemColor,
            _ => throw new NotImplementedException()
        };
}