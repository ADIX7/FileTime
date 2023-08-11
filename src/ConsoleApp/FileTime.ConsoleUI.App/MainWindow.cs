using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.ViewModels;
using FileTime.ConsoleUI.App.Controls;
using FileTime.ConsoleUI.App.Styling;
using FileTime.Core.Enums;
using TerminalUI;
using TerminalUI.Color;
using TerminalUI.Controls;
using TerminalUI.Extensions;
using TerminalUI.Models;
using TerminalUI.ViewExtensions;

namespace FileTime.ConsoleUI.App;

public class MainWindow
{
    private readonly IRootViewModel _rootViewModel;
    private readonly IApplicationContext _applicationContext;
    private readonly ITheme _theme;
    private readonly CommandPalette _commandPalette;

    private readonly Lazy<IView> _root;

    public MainWindow(
        IRootViewModel rootViewModel,
        IApplicationContext applicationContext,
        ITheme theme,
        CommandPalette commandPalette)
    {
        _rootViewModel = rootViewModel;
        _applicationContext = applicationContext;
        _theme = theme;
        _commandPalette = commandPalette;
        _root = new Lazy<IView>(Initialize);
    }

    public IEnumerable<IView> RootViews() => new[]
    {
        _root.Value
    };

    public Grid<IRootViewModel> Initialize()
    {
        var root = new Grid<IRootViewModel>
        {
            Name = "root",
            DataContext = _rootViewModel,
            ApplicationContext = _applicationContext,
            ChildInitializer =
            {
                MainContent(),
                _commandPalette.View()
            }
        };
        return root;
    }

    private Grid<IRootViewModel> MainContent() =>
        new()
        {
            RowDefinitionsObject = "Auto * Auto",
            ChildInitializer =
            {
                new Grid<IRootViewModel>
                {
                    ColumnDefinitionsObject = "Auto * Auto",
                    ChildInitializer =
                    {
                        new StackPanel<IRootViewModel>
                        {
                            Name = "username_panel",
                            Orientation = Orientation.Horizontal,
                            Margin = "0 0 1 0",
                            ChildInitializer =
                            {
                                new TextBlock<IRootViewModel>()
                                    .Setup(t => t.Bind(
                                        t,
                                        root => root.UserName,
                                        tb => tb.Text
                                    )),
                                new TextBlock<IRootViewModel>()
                                    .Setup(t => t.Bind(
                                        t,
                                        root => root.MachineName,
                                        tb => tb.Text,
                                        t => $"@{t}"
                                    ))
                            }
                        },
                        new TextBlock<IRootViewModel>
                            {
                                Foreground = _theme.ContainerColor,
                                Extensions =
                                {
                                    new GridPositionExtension(1, 0)
                                }
                            }
                            .Setup(t => t.Bind(
                                t,
                                root => root.AppState.SelectedTab.Value.CurrentLocation.Value.FullName.Path,
                                tb => tb.Text
                            )),
                        TabControl()
                            .WithExtension(new GridPositionExtension(2, 0))
                    }
                },
                new Grid<IRootViewModel>
                {
                    ColumnDefinitionsObject = "* 4* 4*",
                    Extensions =
                    {
                        new GridPositionExtension(0, 1)
                    },
                    ChildInitializer =
                    {
                        ParentsItemsView().WithExtension(new GridPositionExtension(0, 0)),
                        SelectedItemsView().WithExtension(new GridPositionExtension(1, 0)),
                        SelectedsItemsView().WithExtension(new GridPositionExtension(2, 0)),
                    }
                },
                new Grid<IRootViewModel>
                {
                    Extensions =
                    {
                        new GridPositionExtension(0, 2)
                    },
                    ChildInitializer =
                    {
                        PossibleCommands()
                    }
                }
            }
        };

    private IView<IRootViewModel> PossibleCommands()
    {
        //TODO: Create and use DataGrid
        var commandBindings = new ListView<IRootViewModel, IPossibleCommandEntryViewModel>
        {
            ItemTemplate = _ =>
            {
                var grid = new Grid<IPossibleCommandEntryViewModel>
                {
                    ColumnDefinitionsObject = "10 *",
                    ChildInitializer =
                    {
                        new TextBlock<IPossibleCommandEntryViewModel>()
                            .Setup(t =>
                                t.Bind(
                                    t,
                                    dc => dc.KeysText,
                                    tb => tb.Text)
                            ),
                        new TextBlock<IPossibleCommandEntryViewModel>
                        {
                            Extensions =
                            {
                                new GridPositionExtension(1, 0)
                            }
                        }.Setup(t =>
                            t.Bind(
                                t,
                                dc => dc.Title,
                                tb => tb.Text)
                        )
                    }
                };

                return grid;
            }
        };

        commandBindings.Bind(
            commandBindings,
            root => root.PossibleCommands.PossibleCommands,
            v => v.ItemsSource,
            d => d);

        return commandBindings;
    }

    private IView<IRootViewModel> TabControl()
    {
        var tabList = new ListView<IRootViewModel, ITabViewModel>
        {
            Orientation = Orientation.Horizontal,
            ItemTemplate = item =>
            {
                var textBlock = item.CreateChild<TextBlock<ITabViewModel>>();
                textBlock.Foreground = _theme.DefaultForegroundColor;

                textBlock.Bind(
                    textBlock,
                    dc => dc.TabNumber.ToString(),
                    tb => tb.Text,
                    value => $" {value}",
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
            root => root.AppState.Tabs,
            v => v.ItemsSource);

        return tabList;
    }

    private ListView<IRootViewModel, IItemViewModel> SelectedItemsView()
    {
        var list = new ListView<IRootViewModel, IItemViewModel>
        {
            ListPadding = 8
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
            root => root.AppState.SelectedTab.Value.CurrentItems.Value,
            v => v.ItemsSource);

        list.Bind(
            list,
            root => root.AppState.SelectedTab.Value.CurrentSelectedItem.Value,
            v => v.SelectedItem);

        return list;
    }

    private ListView<IRootViewModel, IItemViewModel> SelectedsItemsView()
    {
        var list = new ListView<IRootViewModel, IItemViewModel>
        {
            ListPadding = 8
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
            root => root.AppState.SelectedTab.Value.SelectedsChildren.Value,
            v => v.ItemsSource);

        return list;
    }

    private ListView<IRootViewModel, IItemViewModel> ParentsItemsView()
    {
        var list = new ListView<IRootViewModel, IItemViewModel>
        {
            ListPadding = 8
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
            root => root.AppState.SelectedTab.Value.ParentsChildren.Value,
            v => v.ItemsSource);

        return list;
    }

    private IColor? ToForegroundColor(ItemViewMode viewMode, AbsolutePathType absolutePathType) =>
        (viewMode, absolutePathType) switch
        {
            (ItemViewMode.Default, AbsolutePathType.Container) => _theme.ContainerColor,
            (ItemViewMode.Alternative, AbsolutePathType.Container) => _theme.ContainerColor,
            (ItemViewMode.Default, _) => _theme.ElementColor,
            (ItemViewMode.Alternative, _) => _theme.ElementColor,
            (ItemViewMode.Selected, _) => _theme.SelectedItemColor,
            (ItemViewMode.Marked, _) => _theme.MarkedItemForegroundColor,
            (ItemViewMode.MarkedSelected, _) => _theme.SelectedItemColor,
            (ItemViewMode.MarkedAlternative, _) => _theme.MarkedItemForegroundColor,
            _ => throw new NotImplementedException()
        };

    private IColor? ToBackgroundColor(ItemViewMode viewMode, AbsolutePathType absolutePathType)
        => (viewMode, absolutePathType) switch
        {
            (ItemViewMode.Default, _) => _theme.DefaultBackgroundColor,
            (ItemViewMode.Alternative, _) => _theme.DefaultBackgroundColor,
            (ItemViewMode.Selected, _) => ToForegroundColor(ItemViewMode.Default, absolutePathType)?.AsBackground(),
            (ItemViewMode.Marked, _) => _theme.MarkedItemBackgroundColor,
            (ItemViewMode.MarkedSelected, _) => ToForegroundColor(ItemViewMode.Marked, absolutePathType)?.AsBackground(),
            (ItemViewMode.MarkedAlternative, _) => _theme.MarkedItemBackgroundColor,
            _ => throw new NotImplementedException()
        };
}