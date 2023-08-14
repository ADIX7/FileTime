﻿using System.Collections.Specialized;
using System.ComponentModel;
using FileTime.App.Core.Models.Enums;
using FileTime.App.Core.ViewModels;
using FileTime.ConsoleUI.App.Controls;
using FileTime.ConsoleUI.App.Styling;
using FileTime.Core.Enums;
using FileTime.Core.Interactions;
using GeneralInputKey;
using TerminalUI;
using TerminalUI.Color;
using TerminalUI.Controls;
using TerminalUI.Extensions;
using TerminalUI.Models;
using TerminalUI.Traits;
using TerminalUI.ViewExtensions;

namespace FileTime.ConsoleUI.App;

public class MainWindow
{
    private readonly IRootViewModel _rootViewModel;
    private readonly IApplicationContext _applicationContext;
    private readonly ITheme _theme;
    private readonly CommandPalette _commandPalette;
    private readonly Lazy<IView> _root;

    private ItemsControl<IRootViewModel, IInputElement> _readInputs = null!;
    private IInputElement? _inputElementToFocus;
    private Action? _readInputChildHandlerUnsubscriber;

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

        rootViewModel.FocusReadInputElement += element =>
        {
            _inputElementToFocus = element;
            UpdateReadInputsFocus();
        };
    }

    private void UpdateReadInputsFocus()
    {
        foreach (var readInputsChild in _readInputs.Children)
        {
            if (readInputsChild.DataContext == _inputElementToFocus)
            {
                if (FindFocusable(readInputsChild) is { } focusable)
                {
                    focusable.Focus();
                    _inputElementToFocus = null;
                    break;
                }
            }
        }

        IFocusable? FindFocusable(IView view)
        {
            if (view is IFocusable focusable) return focusable;
            foreach (var viewVisualChild in view.VisualChildren)
            {
                if (FindFocusable(viewVisualChild) is { } focusableChild)
                    return focusableChild;
            }

            return null;
        }
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
            Foreground = _theme.DefaultForegroundColor,
            ChildInitializer =
            {
                MainContent(),
                _commandPalette.View(),
                Dialogs(),
            }
        };

        ((INotifyPropertyChanged) _readInputs).PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ItemsControl<object, object>.Children))
            {
                _readInputChildHandlerUnsubscriber?.Invoke();
                UpdateReadInputsFocus();
                if (_readInputs.Children is INotifyCollectionChanged notifyCollectionChanged)
                {
                    notifyCollectionChanged.CollectionChanged += NotifyCollectionChangedEventHandler;
                    _readInputChildHandlerUnsubscriber = () => { notifyCollectionChanged.CollectionChanged -= NotifyCollectionChangedEventHandler; };
                }

                void NotifyCollectionChangedEventHandler(
                    object? sender,
                    NotifyCollectionChangedEventArgs e)
                {
                    UpdateReadInputsFocus();
                }
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

    private IView<IRootViewModel> Dialogs()
    {
        var root = new Border<IRootViewModel>()
        {
            Margin = 5,
            BorderThickness = 1,
            Content = new Grid<IRootViewModel>()
            {
                ChildInitializer =
                {
                    ReadInputs()
                }
            }
        };

        root.Bind(
            root,
            d => d.DialogService.ReadInput.Value != null,
            v => v.IsVisible);
        return root;
    }

    private ItemsControl<IRootViewModel, IInputElement> ReadInputs()
    {
        var readInputs = new ItemsControl<IRootViewModel, IInputElement>
            {
                ItemTemplate = () =>
                {
                    var root = new Grid<IInputElement>
                    {
                        ColumnDefinitionsObject = "* *",
                        ChildInitializer =
                        {
                            new TextBlock<IInputElement>()
                                .Setup(t => t.Bind(
                                    t,
                                    c => c.Label,
                                    tb => tb.Text
                                )),
                            new Grid<IInputElement>()
                            {
                                Extensions =
                                {
                                    new GridPositionExtension(1, 0)
                                },
                                ChildInitializer =
                                {
                                    new Border<IInputElement>
                                        {
                                            Content =
                                                new TextBox<IInputElement>()
                                                    .Setup(t => t.Bind(
                                                        t,
                                                        d => ((TextInputElement) d).Value,
                                                        tb => tb.Text,
                                                        v => v ?? string.Empty,
                                                        fallbackValue: string.Empty
                                                    ))
                                                    .WithTextHandler((tb, t) =>
                                                    {
                                                        if (tb.DataContext is TextInputElement textInputElement)
                                                            textInputElement.Value = t;
                                                    })
                                        }
                                        .Setup(t => t.Bind(
                                            t,
                                            d => d.Type == InputType.Text,
                                            tb => tb.IsVisible
                                        )),
                                    new Border<IInputElement>
                                        {
                                            Content =
                                                new TextBox<IInputElement>
                                                    {
                                                        PasswordChar = '*'
                                                    }
                                                    .Setup(t => t.Bind(
                                                        t,
                                                        d => ((PasswordInputElement) d).Value,
                                                        tb => tb.Text,
                                                        v => v ?? string.Empty,
                                                        fallbackValue: string.Empty
                                                    ))
                                                    .WithTextHandler((tb, t) =>
                                                    {
                                                        if (tb.DataContext is PasswordInputElement textInputElement)
                                                            textInputElement.Value = t;
                                                    })
                                        }
                                        .Setup(t => t.Bind(
                                            t,
                                            d => d.Type == InputType.Password,
                                            tb => tb.IsVisible
                                        ))
                                    //TODO: OptionInputElement
                                }
                            }
                        }
                    };

                    return root;
                }
            }
            .Setup(t => t.Bind(
                t,
                d => d.DialogService.ReadInput.Value.Inputs,
                c => c.ItemsSource,
                v => v
            ));

        readInputs.WithKeyHandler((_, e) =>
        {
            if (e.Key == Keys.Enter)
            {
                if (_rootViewModel.DialogService.ReadInput.Value is { } readInputsViewModel)
                    readInputsViewModel.Process();
                
                e.Handled = true;
            }
            else if (e.Key == Keys.Escape)
            {
                if (_rootViewModel.DialogService.ReadInput.Value is { } readInputsViewModel)
                    readInputsViewModel.Cancel();
                
                e.Handled = true;
            }
        });

        _readInputs = readInputs;

        return readInputs;
    }
}