using FileTime.App.CommandPalette.Services;
using FileTime.App.CommandPalette.ViewModels;
using FileTime.ConsoleUI.App.Styling;
using GeneralInputKey;
using TerminalUI.Color;
using TerminalUI.Controls;
using TerminalUI.Extensions;
using TerminalUI.Models;
using TerminalUI.ViewExtensions;

namespace FileTime.ConsoleUI.App.Controls;

public class CommandPalette
{
    private readonly ITheme _theme;
    private readonly ICommandPaletteService _commandPaletteService;

    public CommandPalette(ITheme theme, ICommandPaletteService commandPaletteService)
    {
        _theme = theme;
        _commandPaletteService = commandPaletteService;
    }

    public Border<IRootViewModel> View()
    {
        var inputTextBox = new TextBox<IRootViewModel>()
            .WithKeyHandler((sender, k) =>
            {
                if (k.Key == Keys.Escape)
                {
                    _commandPaletteService.CloseCommandPalette();
                    sender.Text = String.Empty;
                }

                if (!k.Handled)
                {
                    sender.DataContext?.CommandPalette.HandleKeyDown(k);
                }

                if (!k.Handled)
                {
                    sender.DataContext?.CommandPalette.HandleKeyUp(k);
                }

                if (k.Key == Keys.Enter)
                {
                    sender.Text = String.Empty;
                }
            })
            .WithTextHandler((sender, text) =>
            {
                if (sender.DataContext is not null)
                {
                    sender.DataContext.CommandPalette.SearchText = text;
                }
            });

        var root = new Border<IRootViewModel>
        {
            Margin = 5,
            Padding = 1,
            MaxWidth = 50,
            Fill = SpecialColor.None,
            Content = new Grid<IRootViewModel>
            {
                RowDefinitionsObject = "Auto *",
                ChildInitializer =
                {
                    new Border<IRootViewModel>
                    {
                        Margin = new Thickness(0, 0, 0, 1),
                        Content = inputTextBox
                    },
                    new ListView<IRootViewModel, ICommandPaletteEntryViewModel>
                        {
                            Extensions =
                            {
                                new GridPositionExtension(0, 1)
                            },
                            ItemTemplate = item =>
                            {
                                var root = new Grid<ICommandPaletteEntryViewModel>
                                {
                                    ColumnDefinitionsObject = "* Auto",
                                    ChildInitializer =
                                    {
                                        new TextBlock<ICommandPaletteEntryViewModel>()
                                            .Setup(t => t.Bind(
                                                t,
                                                d => d.Title,
                                                t => t.Text)),
                                        new TextBlock<ICommandPaletteEntryViewModel>
                                            {
                                                Extensions =
                                                {
                                                    new GridPositionExtension(1, 0)
                                                }
                                            }
                                            .Setup(t => t.Bind(
                                                t,
                                                d => d.Shortcuts,
                                                t => t.Text))
                                    }
                                };

                                item.Bind(
                                    item.Parent,
                                    d => d.CommandPalette.SelectedItem == item.DataContext ? _theme.ListViewItemTheme.SelectedBackgroundColor : null,
                                    t => t.Background
                                );

                                item.Bind(
                                    item.Parent,
                                    d => d.CommandPalette.SelectedItem == item.DataContext ? _theme.ListViewItemTheme.SelectedForegroundColor : null,
                                    t => t.Foreground
                                );

                                return root;
                            }
                        }.Setup(t => t.Bind(
                            t,
                            d => d.CommandPalette.FilteredMatches,
                            t => t.ItemsSource
                        ))
                        .Setup(t => t.Bind(
                            t,
                            d => d.CommandPalette.SelectedItem,
                            t => t.SelectedItem
                        ))
                }
            }
        };

        root.WithPropertyChangedHandler(r => r.IsVisible,
            (_, isVisible) =>
            {
                if (isVisible)
                {
                    inputTextBox.Focus();
                }
                else
                {
                    inputTextBox.UnFocus();
                }
            });

        root.Bind(
            root,
            d => d.CommandPalette.ShowWindow.Value,
            t => t.IsVisible,
            r => r);

        return root;
    }
}