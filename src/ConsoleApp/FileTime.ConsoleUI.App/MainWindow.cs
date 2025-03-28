﻿using System.Diagnostics;
using System.Globalization;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.ViewModels;
using FileTime.ConsoleUI.App.Configuration;
using FileTime.ConsoleUI.App.Controls;
using FileTime.ConsoleUI.App.Services;
using FileTime.ConsoleUI.App.Styling;
using FileTime.Core.Enums;
using FileTime.Core.Models;
using FileTime.Core.Traits;
using Humanizer.Bytes;
using Microsoft.Extensions.Options;
using TerminalUI;
using TerminalUI.Color;
using TerminalUI.Controls;
using TerminalUI.Extensions;
using TerminalUI.Models;
using TerminalUI.TextFormat;
using TerminalUI.ViewExtensions;

namespace FileTime.ConsoleUI.App;

public class MainWindow
{
    private readonly struct ItemViewRenderOptions
    {
        public readonly bool ShowAttributes;

        public ItemViewRenderOptions(bool showAttributes = false)
        {
            ShowAttributes = showAttributes;
        }
    }

    private readonly IRootViewModel _rootViewModel;
    private readonly IApplicationContext _applicationContext;
    private readonly ITheme _theme;
    private readonly CommandPalette _commandPalette;
    private readonly FrequencyNavigation _frequencyNavigation;
    private readonly Dialogs _dialogs;
    private readonly Timeline _timeline;
    private readonly ItemPreviews _itemPreviews;
    private readonly IIconProvider _iconProvider;
    private readonly IOptions<ConsoleApplicationConfiguration> _consoleApplicationConfiguration;
    private readonly Lazy<IView> _root;

    public MainWindow(
        IRootViewModel rootViewModel,
        IApplicationContext applicationContext,
        IThemeProvider themeProvider,
        CommandPalette commandPalette,
        FrequencyNavigation frequencyNavigation,
        Dialogs dialogs,
        Timeline timeline,
        ItemPreviews itemPreviews,
        IIconProvider iconProvider,
        IOptions<ConsoleApplicationConfiguration> consoleApplicationConfiguration)
    {
        _rootViewModel = rootViewModel;
        _applicationContext = applicationContext;
        _theme = themeProvider.CurrentTheme;
        _commandPalette = commandPalette;
        _frequencyNavigation = frequencyNavigation;
        _dialogs = dialogs;
        _timeline = timeline;
        _itemPreviews = itemPreviews;
        _iconProvider = iconProvider;
        _consoleApplicationConfiguration = consoleApplicationConfiguration;
        _root = new Lazy<IView>(Initialize);
    }

    public IEnumerable<IView> RootViews() => new[]
    {
        _root.Value
    };

    private Grid<IRootViewModel> Initialize()
    {
        var root = new Grid<IRootViewModel>
        {
            Name = "root",
            DataContext = _rootViewModel,
            ApplicationContext = _applicationContext,
            Foreground = _theme.DefaultForegroundColor,
            ChildInitializer =
            {
                MainContent(),
                _commandPalette.View(),
                _frequencyNavigation.View(),
                _dialogs.View(),
            }
        };
        return root;
    }

    private Grid<IRootViewModel> MainContent() =>
        new()
        {
            RowDefinitionsObject = "Auto * Auto Auto Auto Auto Auto",
            ChildInitializer =
            {
                new Grid<IRootViewModel>
                {
                    ColumnDefinitionsObject = "Auto * Auto Auto",
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
                                        root => root!.UserName,
                                        tb => tb.Text
                                    )),
                                new TextBlock<IRootViewModel>()
                                    .Setup(t => t.Bind(
                                        t,
                                        root => root!.MachineName,
                                        tb => tb.Text,
                                        v => $"@{v}"
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
                                root => root!.AppState.SelectedTab.Value!.CurrentLocation.Value!.FullName!.Path,
                                tb => tb.Text
                            )),
                        new StackPanel<IRootViewModel>
                        {
                            Margin = "2 0 0 0",
                            Extensions = {new GridPositionExtension(2, 0)},
                            Orientation = Orientation.Horizontal,
                            ChildInitializer =
                            {
                                new TextBlock<IRootViewModel>
                                {
                                    Text = _consoleApplicationConfiguration.Value.AdminModeIcon ??
                                           (_consoleApplicationConfiguration.Value.DisableUtf8 ? "A+ " : "\ud83d\udd11"),
                                    AsciiOnly = false
                                }.Setup(t => t.Bind(
                                    t,
                                    dc => dc!.AdminElevationManager.IsAdminInstanceRunning,
                                    tb => tb.IsVisible)),
                                new TextBlock<IRootViewModel>
                                {
                                    Text = _consoleApplicationConfiguration.Value.ClipboardSingleIcon ??
                                           (_consoleApplicationConfiguration.Value.DisableUtf8 ? "C " : "\ud83d\udccb"),
                                    AsciiOnly = false
                                }.Setup(t => t.Bind(
                                    t,
                                    dc => dc!.ClipboardService.Content.Count == 1,
                                    tb => tb.IsVisible)),
                                new TextBlock<IRootViewModel>
                                {
                                    Text = _consoleApplicationConfiguration.Value.ClipboardMultipleIcon ??
                                           (_consoleApplicationConfiguration.Value.DisableUtf8 ? "CC " : "\ud83d\udccb+"),
                                    AsciiOnly = false
                                }.Setup(t => t.Bind(
                                    t,
                                    dc => dc!.ClipboardService.Content.Count > 1,
                                    tb => tb.IsVisible)),
                                new TextBlock<IRootViewModel>
                                {
                                    Text = /*_consoleApplicationConfiguration.Value.ClipboardMultipleIcon ??*/
                                           (_consoleApplicationConfiguration.Value.DisableUtf8 ? "DB " : "\udb80\udce4"),
                                    AsciiOnly = false
                                }.Setup(t => t.Bind(
                                    t,
                                    dc => dc.IsDebuggerAttached.Value,
                                    tb => tb.IsVisible))
                            }
                        },
                        TabControl()
                            .WithExtension(new GridPositionExtension(3, 0))
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
                        new Grid<IRootViewModel>
                        {
                            Extensions =
                            {
                                new GridPositionExtension(1, 0)
                            },
                            ChildInitializer =
                            {
                                SelectedItemsView(),
                                new TextBlock<IRootViewModel>
                                {
                                    Text = "Empty",
                                    Foreground = _theme.ErrorForegroundColor,
                                    TextAlignment = TextAlignment.Center
                                }.Setup(t => t.Bind(
                                    t,
                                    dc => dc!.AppState.SelectedTab.Value!.CurrentItems.Value!.Count == 0,
                                    tb => tb.IsVisible,
                                    fallbackValue: true
                                ))
                            }
                        },
                        new Grid<IRootViewModel>
                        {
                            Extensions =
                            {
                                new GridPositionExtension(2, 0)
                            },
                            ChildInitializer =
                            {
                                SelectedsItemsView(),
                                _itemPreviews.View()
                            }
                        }
                    }
                },
                new ItemsControl<IRootViewModel, string>
                    {
                        MaxHeight = 5,
                        Extensions =
                        {
                            new GridPositionExtension(0, 2)
                        },
                        ItemTemplate = () =>
                        {
                            return new TextBlock<string>
                                {
                                    Foreground = _theme.WarningForegroundColor
                                }
                                .Setup(t => t.Bind(
                                    t,
                                    dc => dc,
                                    tb => tb.Text));
                        }
                    }
                    .Setup(i => i.Bind(
                        i,
                        dc => dc!.AppState.PopupTexts,
                        ic => ic.ItemsSource
                    )),
                new Grid<IRootViewModel>
                {
                    Extensions =
                    {
                        new GridPositionExtension(0, 3)
                    },
                    ChildInitializer =
                    {
                        PossibleCommands()
                    }
                },
                _timeline.View().WithExtension(new GridPositionExtension(0, 4)),
                new TextBlock<IRootViewModel>
                    {
                        Extensions =
                        {
                            new GridPositionExtension(0, 5)
                        },
                    }
                    .Setup(t =>
                    {
                        t.Bind(
                            t,
                            dc => dc!.AppState.RapidTravelText.Value,
                            v => v.Text,
                            tb => "Filter: " + tb);

                        t.Bind(
                            t,
                            dc => dc!.AppState.ViewMode.Value == ViewMode.RapidTravel,
                            tb => tb.IsVisible);
                    }),
                StatusLine().WithExtension(new GridPositionExtension(0, 6)),
            }
        };

    private IView<IRootViewModel> StatusLine()
        => new Grid<IRootViewModel>
        {
            ColumnDefinitionsObject = "* Auto",
            ChildInitializer =
            {
                new StackPanel<IRootViewModel>
                {
                    Orientation = Orientation.Horizontal,
                    ChildInitializer =
                    {
                        new TextBlock<IRootViewModel>
                            {
                                Margin = "0 0 1 0",
                            }
                            .Setup(t => t.Bind(
                                t,
                                dc => dc!.AppState.SelectedTab.Value!.CurrentSelectedItem.Value!.Attributes,
                                tb => tb.Text)),
                        new TextBlock<IRootViewModel>
                            {
                                Margin = "0 0 1 0",
                            }
                            .Setup(t => t.Bind(
                                t,
                                dc => dc!.AppState.SelectedTab.Value!.CurrentSelectedItem.Value!.ModifiedAt,
                                tb => tb.Text,
                                v => v.ToString()))
                    }
                },
                new StackPanel<IRootViewModel>()
                {
                    Orientation = Orientation.Horizontal,
                    Extensions = {new GridPositionExtension(1, 0)},
                    ChildInitializer =
                    {
                        new TextBlock<IRootViewModel>()
                            .Setup(t =>
                            {
                                t.Bind(
                                    t,
                                    dc => dc!.VolumeSizeInfo.Value,
                                    tb => tb.IsVisible,
                                    v => v.HasValue
                                );
                                t.Bind(
                                    t,
                                    dc => dc!.VolumeSizeInfo.Value,
                                    tb => tb.Text,
                                    v => v.HasValue
                                        ? $"{ByteSize.FromBytes(v.Value.FreeSize)} / {ByteSize.FromBytes(v.Value.TotalSize)} free"
                                        : string.Empty
                                );
                            }),
                        new StackPanel<IRootViewModel>
                            {
                                Margin = "2 0 0 0",
                                Orientation = Orientation.Horizontal,
                                ChildInitializer =
                                {
                                    new TextBlock<IRootViewModel>()
                                        .Setup(t => t.Bind(
                                            t,
                                            dc => dc!.AppState.SelectedTab.Value!.CurrentSelectedItemIndex.Value,
                                            tb => tb.Text,
                                            v => v is null or < 0 ? "?" : $"{v + 1}")),
                                    new TextBlock<IRootViewModel>
                                        {
                                            Foreground = _theme.MarkedItemForegroundColor,
                                            TextFormat = new AnsiFormat
                                            {
                                                IsBold = true
                                            }
                                        }
                                        .Setup(t =>
                                        {
                                            t.Bind(
                                                t,
                                                dc => dc!.AppState.SelectedTab.Value!.MarkedItems.Value!.Count,
                                                tb => tb.Text,
                                                v => $"/{v}");

                                            t.Bind(
                                                t,
                                                dc => dc!.AppState.SelectedTab.Value!.MarkedItems.Value!.Count > 0,
                                                tb => tb.IsVisible);
                                        }),
                                    new TextBlock<IRootViewModel>()
                                        .Setup(t => t.Bind(
                                            t,
                                            dc => dc!.AppState.SelectedTab.Value!.CurrentItems.Value!.Count,
                                            tb => tb.Text,
                                            v => $"/{v}")),
                                }
                            }
                            .Setup(s => s.Bind(
                                s,
                                dc => dc!.AppState.SelectedTab.Value!.CurrentItems.Value!.Count > 0,
                                sp => sp.IsVisible)),
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
                                    dc => dc!.KeysText,
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
                                dc => dc!.Title,
                                tb => tb.Text)
                        )
                    }
                };

                return grid;
            }
        };

        commandBindings.Bind(
            commandBindings,
            dc => dc!.PossibleCommands.PossibleCommands,
            v => v.ItemsSource);

        return commandBindings;
    }

    private IView<IRootViewModel> TabControl()
    {
        var tabList = new ListView<IRootViewModel, ITabViewModel>
        {
            Margin = "1 0 0 0",
            Orientation = Orientation.Horizontal,
            ItemTemplate = item =>
            {
                var textBlock = item.CreateChild<TextBlock<ITabViewModel>>();

                textBlock.Bind(
                    textBlock,
                    dc => dc!.TabNumber.ToString(),
                    tb => tb.Text,
                    value => $" {value}",
                    fallbackValue: "?");

                textBlock.Bind(
                    textBlock,
                    dc => dc!.IsSelected.Value ? _theme.SelectedTabBackgroundColor : null,
                    tb => tb.Background,
                    fallbackValue: null
                );
                return textBlock;
            }
        };

        tabList.Bind(
            tabList,
            root => root!.AppState.Tabs,
            tl => tl.ItemsSource);

        return tabList;
    }

    private ListView<IRootViewModel, IItemViewModel> SelectedItemsView()
    {
        var list = new ListView<IRootViewModel, IItemViewModel>
        {
            ListPadding = 8,
            Margin = "1 0 1 0"
        };

        list.ItemTemplate = _ => ItemItemTemplate(new ItemViewRenderOptions(true));

        list.Bind(
            list,
            dc => dc!.AppState.SelectedTab.Value!.CurrentItems.Value,
            lv => lv.ItemsSource);

        list.Bind(
            list,
            dc => dc!.AppState.SelectedTab.Value!.CurrentSelectedItem.Value,
            lv => lv.SelectedItem);

        return list;
    }

    private ListView<IRootViewModel, IItemViewModel> SelectedsItemsView()
    {
        var list = new ListView<IRootViewModel, IItemViewModel>
        {
            ListPadding = 8,
            ItemTemplate = _ => ItemItemTemplate(new ItemViewRenderOptions())
        };

        list.Bind(
            list,
            dc =>
                dc!.AppState.SelectedTab.Value!.SelectedsChildren.Value!.Count > 0
                && dc.ItemPreviewService.ItemPreview.Value == null,
            l => l.IsVisible,
            fallbackValue: false);

        list.Bind(
            list,
            dc => dc!.AppState.SelectedTab.Value!.SelectedsChildren.Value,
            v => v.ItemsSource,
            fallbackValue: null);

        return list;
    }

    private ListView<IRootViewModel, IItemViewModel> ParentsItemsView()
    {
        var list = new ListView<IRootViewModel, IItemViewModel>
        {
            ListPadding = 8,
            ItemTemplate = _ => ItemItemTemplate(new ItemViewRenderOptions())
        };

        list.Bind(
            list,
            dc => dc!.AppState.SelectedTab.Value!.ParentsChildren.Value,
            lv => lv.ItemsSource);

        return list;
    }

    private IView<IItemViewModel> ItemItemTemplate(ItemViewRenderOptions options)
    {
        var root = new Grid<IItemViewModel>
        {
            ChildInitializer =
            {
                new Rectangle<IItemViewModel>(),
                new Grid<IItemViewModel>
                {
                    Margin = "1 0 1 0",
                    ColumnDefinitionsObject = "2 * Auto",
                    ChildInitializer =
                    {
                        new TextBlock<IItemViewModel>()
                        {
                            AsciiOnly = false
                        }
                            .Setup(t =>
                            {
                                t.Bind(
                                    t,
                                    dc => dc == null ? string.Empty : _iconProvider.GetImage(dc.BaseItem),
                                    tb => tb.Text
                                );
                            }),
                        new TextBlock<IItemViewModel>()
                            .Setup(t =>
                            {
                                t.Bind(
                                    t,
                                    dc => dc == null ? string.Empty : dc.DisplayNameText,
                                    tb => tb.Text
                                );
                            }).WithExtension(new GridPositionExtension(1, 0)),
                        new StackPanel<IItemViewModel>
                        {
                            Orientation = Orientation.Horizontal,
                            Extensions = {new GridPositionExtension(2, 0)},
                            ChildInitializer =
                            {
                                new TextBlock<IItemViewModel>()
                                    .Setup(t =>
                                    {
                                        if (!options.ShowAttributes) return;
                                        t.Bind(
                                            t,
                                            dc => dc is ISizeProvider
                                                ? ((ISizeProvider) dc).Size.Value
                                                : ((ISizeProvider) dc!.BaseItem!).Size.Value,
                                            tb => tb.Text,
                                            v =>
                                            {
                                                var b = ByteSize.FromBytes(v);

                                                return $"{b.LargestWholeNumberValue:0.#} " + b.GetLargestWholeNumberSymbol(NumberFormatInfo.CurrentInfo).First();
                                            });
                                    }),

                                new TextBlock<IItemViewModel>()
                                    .Setup(t =>
                                    {
                                        t.Bind(
                                            t,
                                            dc => dc!.BaseItem!.Type == AbsolutePathType.Container,
                                            tb => tb.IsVisible);

                                        if (!options.ShowAttributes) return;
                                        t.Bind(
                                            t,
                                            dc => ((IContainer) dc!.BaseItem!).Items.Count,
                                            tb => tb.Text,
                                            v => $" {v,4}");
                                    })
                            }
                        },
                    }
                }
            }
        };

        root.Bind(
            root,
            dc => dc == null
                ? _theme.DefaultForegroundColor
                : ToForegroundColor(dc.ViewMode.Value, dc.BaseItem!.Type),
            tb => tb.Foreground
        );

        root.Bind(
            root,
            dc => dc == null
                ? _theme.DefaultBackgroundColor
                : ToBackgroundColor(dc.ViewMode.Value, dc.BaseItem!.Type),
            tb => tb.Background
        );

        return root;
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