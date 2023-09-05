using FileTime.App.FrequencyNavigation.Services;
using FileTime.ConsoleUI.App.Styling;
using GeneralInputKey;
using TerminalUI.Color;
using TerminalUI.Controls;
using TerminalUI.Extensions;
using TerminalUI.Models;
using TerminalUI.ViewExtensions;

namespace FileTime.ConsoleUI.App.Controls;

public class FrequencyNavigation
{
    private readonly ITheme _theme;
    private readonly IFrequencyNavigationService _frequencyNavigationService;

    public FrequencyNavigation(ITheme theme, IFrequencyNavigationService frequencyNavigationService)
    {
        _theme = theme;
        _frequencyNavigationService = frequencyNavigationService;
    }
    
    public IView<IRootViewModel> View()
    {
        var inputTextBox = new TextBox<IRootViewModel>()
            .WithKeyHandler((sender, k) =>
            {
                if (k.Key == Keys.Escape)
                {
                    _frequencyNavigationService.CloseNavigationWindow();
                    sender.Text = String.Empty;
                }

                if (!k.Handled)
                {
                    sender.DataContext?.FrequencyNavigation.HandleKeyDown(k);
                }

                if (!k.Handled)
                {
                    sender.DataContext?.FrequencyNavigation.HandleKeyUp(k);
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
                    sender.DataContext.FrequencyNavigation.SearchText = text;
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
                    new ListView<IRootViewModel, string>
                        {
                            Extensions =
                            {
                                new GridPositionExtension(0, 1)
                            },
                            ItemTemplate = item =>
                            {
                                var root = new Grid<string>
                                {
                                    ChildInitializer =
                                    {
                                        new TextBlock<string>()
                                            .Setup(t => t.Bind(
                                                t,
                                                d => d,
                                                tb => tb.Text)),
                                    }
                                };

                                item.Bind(
                                    item.Parent,
                                    dc => dc!.FrequencyNavigation.SelectedItem == item.DataContext ? _theme.ListViewItemTheme.SelectedBackgroundColor : null,
                                    t => t.Background
                                );

                                item.Bind(
                                    item.Parent,
                                    dc => dc!.FrequencyNavigation.SelectedItem == item.DataContext ? _theme.ListViewItemTheme.SelectedForegroundColor : null,
                                    t => t.Foreground
                                );

                                return root;
                            }
                        }.Setup(t => t.Bind(
                            t,
                            dc => dc!.FrequencyNavigation.FilteredMatches,
                            tb => tb.ItemsSource
                        ))
                        .Setup(t => t.Bind(
                            t,
                            dc => dc!.FrequencyNavigation.SelectedItem,
                            tb => tb.SelectedItem
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
            dc => dc!.FrequencyNavigation.ShowWindow.Value,
            t => t.IsVisible,
            r => r);

        return root;
    }
}