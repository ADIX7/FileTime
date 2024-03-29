﻿using System.Collections.Specialized;
using System.ComponentModel;
using FileTime.App.Core.Interactions;
using FileTime.ConsoleUI.App.Styling;
using FileTime.Core.Interactions;
using FileTime.Core.Models;
using GeneralInputKey;
using TerminalUI.Color;
using TerminalUI.Controls;
using TerminalUI.Extensions;
using TerminalUI.Models;
using TerminalUI.TextFormat;
using TerminalUI.Traits;
using TerminalUI.ViewExtensions;

namespace FileTime.ConsoleUI.App.Controls;

public class Dialogs
{
    private readonly IRootViewModel _rootViewModel;
    private ItemsControl<IRootViewModel, IInputElement> _readInputs = null!;
    private IInputElement? _inputElementToFocus;

    private Action? _readInputChildHandlerUnSubscriber;


    private readonly ITextFormat _specialItemNamePartFormat;

    public Dialogs(IRootViewModel rootViewModel, ITheme theme)
    {
        _rootViewModel = rootViewModel;

        _specialItemNamePartFormat = new OrFormat
        {
            Format1 = new AnsiFormat
            {
                IsUnderline = true
            },
            Format2 = new SimpleFormat
            {
                Foreground = theme.DefaultForegroundAccentColor
            }
        };

        rootViewModel.FocusReadInputElement += element =>
        {
            _inputElementToFocus = element;
            UpdateReadInputsFocus();
        };
    }

    public IView<IRootViewModel> View()
    {
        var root = new Grid<IRootViewModel>()
        {
            Margin = 5,
            ChildInitializer =
            {
                ReadInputs(),
                MessageBox()
            }
        };

        return root;
    }

    private IView<IRootViewModel> MessageBox()
    {
        var okButton = new Button<IRootViewModel>
        {
            Margin = "0 0 5 0",
            Content = new TextBlock<IRootViewModel>()
                .Setup(t => t.Bind(
                    t,
                    dc => dc!.DialogService.LastMessageBox.Value!.OkText,
                    tb => tb.Text)),
        }.WithClickHandler(b => b.DataContext?.DialogService.LastMessageBox.Value?.Ok());

        var cancelButton =
            new Button<IRootViewModel>
                {
                    Margin = "0 0 5 0",
                    Content = new TextBlock<IRootViewModel>()
                        .Setup(t => t.Bind(
                            t,
                            dc => dc!.DialogService.LastMessageBox.Value!.CancelText,
                            tb => tb.Text)),
                }
                .Setup(b => b.Bind(
                    b,
                    dc => dc!.DialogService.LastMessageBox.Value!.ShowCancel,
                    bt => bt.IsVisible))
                .WithClickHandler(b => b.DataContext?.DialogService.LastMessageBox.Value?.Cancel());

        var root = new Border<IRootViewModel>
        {
            Margin = 5,
            BorderThickness = 1,
            Background = SpecialColor.None,
            Content = new Grid<IRootViewModel>
            {
                RowDefinitionsObject = "Auto Auto",
                ChildInitializer =
                {
                    new TextBlock<IRootViewModel>()
                        .Setup(t => t.Bind(
                            t,
                            dc => dc!.DialogService.LastMessageBox.Value!.Text,
                            tb => tb.Text)),
                    new StackPanel<IRootViewModel>
                    {
                        Orientation = Orientation.Horizontal,
                        Extensions = {new GridPositionExtension(0, 1)},
                        ChildInitializer =
                        {
                            okButton,
                            cancelButton
                        }
                    }
                }
            }
        };

        root.Bind(
            root,
            dc => dc!.DialogService.LastMessageBox.Value != null,
            br => br.IsVisible,
            fallbackValue: false);

        ((INotifyPropertyChanged) root).PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(IView.IsVisible))
            {
                okButton.Focus();
            }
        };

        return root;
    }

    private IView<IRootViewModel> ReadInputs()
    {
        var root = new Border<IRootViewModel>
        {
            Margin = 5,
            BorderThickness = 1,
            Background = SpecialColor.None,
            Content = new Grid<IRootViewModel>
            {
                RowDefinitionsObject = "Auto Auto",
                ChildInitializer =
                {
                    ReadInputsList(),
                    new ItemsControl<IRootViewModel, IPreviewElement>
                        {
                            ItemTemplate = ReadInputPreviewItemTemplate
                        }
                        .Setup(i => i.Bind(
                            i,
                            dc => dc!.DialogService.ReadInput.Value!.Previews,
                            ic => ic.ItemsSource
                        ))
                        .WithExtension(new GridPositionExtension(0, 1))
                }
            }
        };

        root.Bind(
            root,
            dc => dc!.DialogService.ReadInput.Value != null,
            br => br.IsVisible);

        return root;
    }

    private IView<IPreviewElement> ReadInputPreviewItemTemplate()
    {
        var grid = new Grid<IPreviewElement>
        {
            ChildInitializer =
            {
                new ItemsControl<IPreviewElement, IPreviewElement>
                    {
                        ItemTemplate = ReadInputPreviewItemTemplate
                    }
                    .Setup(i => i.Bind(
                        i,
                        dc => (PreviewType) dc!.PreviewType == PreviewType.PreviewList,
                        ic => ic.IsVisible))
                    .Setup(i => i.Bind(
                        i,
                        dc => ((PreviewList) dc!).Items,
                        ic => ic.ItemsSource)),
                new Grid<IPreviewElement>
                {
                    ColumnDefinitionsObject = "* *",
                    ChildInitializer =
                    {
                        new TextBlock<IPreviewElement>()
                            .Setup(t => t.Bind(
                                t,
                                dc => ((DoubleTextPreview) dc!).Text1,
                                tb => tb.Text
                            )),
                        new TextBlock<IPreviewElement>
                            {
                                Extensions =
                                {
                                    new GridPositionExtension(1, 0)
                                }
                            }
                            .Setup(t => t.Bind(
                                t,
                                dc => ((DoubleTextPreview) dc!).Text2,
                                tb => tb.Text
                            ))
                    }
                }.Setup(g => g.Bind(
                    g,
                    dc => (PreviewType) dc!.PreviewType == PreviewType.DoubleText,
                    gr => gr.IsVisible)),
                new Grid<IPreviewElement>
                {
                    ColumnDefinitionsObject = "* *",
                    ChildInitializer =
                    {
                        new ItemsControl<IPreviewElement, ItemNamePart>
                        {
                            Orientation = Orientation.Horizontal,
                            ItemTemplate = ItemNamePartItemTemplate
                        }.Setup(i => i.Bind(
                            i,
                            dc => ((DoubleItemNamePartListPreview) dc!).ItemNameParts1,
                            c => c.ItemsSource)),
                        new ItemsControl<IPreviewElement, ItemNamePart>
                        {
                            Orientation = Orientation.Horizontal,
                            Extensions =
                            {
                                new GridPositionExtension(1, 0)
                            },
                            ItemTemplate = ItemNamePartItemTemplate
                        }.Setup(i => i.Bind(
                            i,
                            dc => ((DoubleItemNamePartListPreview) dc!).ItemNameParts2,
                            c => c.ItemsSource))
                    }
                }.Setup(g => g.Bind(
                    g,
                    dc => (PreviewType) dc!.PreviewType == PreviewType.DoubleItemNamePartList,
                    gr => gr.IsVisible))
            }
        };

        return grid;

        IView<ItemNamePart> ItemNamePartItemTemplate()
        {
            var textBlock = new TextBlock<ItemNamePart>();
            textBlock.Bind(
                textBlock,
                dc => dc!.Text,
                tb => tb.Text
            );
            textBlock.Bind(
                textBlock,
                dc => dc!.IsSpecial ? _specialItemNamePartFormat : null,
                tb => tb.TextFormat
            );

            return textBlock;
        }
    }

    private IView<IRootViewModel> ReadInputsList()
    {
        var readInputs = new ItemsControl<IRootViewModel, IInputElement>
            {
                IsFocusBoundary = true,
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
                                    dc => dc!.Label,
                                    tb => tb.Text
                                )),
                            new Grid<IInputElement>
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
                                                        dc => ((TextInputElement) dc!).Value,
                                                        tb => tb.Text,
                                                        v => v ?? string.Empty,
                                                        fallbackValue: string.Empty
                                                    ))
                                                    .Setup(t => t.Bind(
                                                        t,
                                                        dc => ((TextInputElement) dc!).Label,
                                                        tb => tb.Name))
                                                    .WithTextHandler((tb, t) =>
                                                    {
                                                        if (tb.DataContext is TextInputElement textInputElement)
                                                            textInputElement.Value = t;
                                                    })
                                        }
                                        .Setup(t => t.Bind(
                                            t,
                                            dc => dc!.Type == InputType.Text,
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
                                                        d => ((PasswordInputElement) d!).Value,
                                                        tb => tb.Text,
                                                        v => v ?? string.Empty,
                                                        fallbackValue: string.Empty
                                                    ))
                                                    .Setup(t => t.Bind(
                                                        t,
                                                        d => ((PasswordInputElement) d!).Label,
                                                        tb => tb.Name))
                                                    .WithTextHandler((tb, t) =>
                                                    {
                                                        if (tb.DataContext is PasswordInputElement textInputElement)
                                                            textInputElement.Value = t;
                                                    })
                                        }
                                        .Setup(t => t.Bind(
                                            t,
                                            dc => dc!.Type == InputType.Password,
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
                dc => dc!.DialogService.ReadInput.Value!.Inputs,
                ic => ic.ItemsSource
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

        ((INotifyPropertyChanged) _readInputs).PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ItemsControl<object, object>.Children))
            {
                _readInputChildHandlerUnSubscriber?.Invoke();

                if (_readInputs.Children.Count > 0)
                {
                    UpdateReadInputsFocus();
                }
                else
                {
                    _inputElementToFocus = null;
                }

                if (_readInputs.Children is INotifyCollectionChanged notifyCollectionChanged)
                {
                    notifyCollectionChanged.CollectionChanged += NotifyCollectionChangedEventHandler;
                    _readInputChildHandlerUnSubscriber = () => { notifyCollectionChanged.CollectionChanged -= NotifyCollectionChangedEventHandler; };
                }

                void NotifyCollectionChangedEventHandler(
                    object? sender,
                    NotifyCollectionChangedEventArgs _)
                {
                    UpdateReadInputsFocus();
                }
            }
        };

        return readInputs;
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
}