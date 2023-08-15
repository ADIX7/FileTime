using FileTime.App.Core.ViewModels.Timeline;
using FileTime.ConsoleUI.App.Styling;
using TerminalUI.Controls;
using TerminalUI.Extensions;
using TerminalUI.Models;
using TerminalUI.Styling.Controls;
using TerminalUI.ViewExtensions;

namespace FileTime.ConsoleUI.App.Controls;

public class Timeline
{
    private readonly IProgressBarTheme? _progressBarTheme;

    public Timeline(ITheme theme)
    {
        _progressBarTheme = theme.ConsoleTheme?.ControlThemes.ProgressBar;
    }

    public IView<IRootViewModel> View()
    {
        var root = new Grid<IRootViewModel>
        {
            ChildInitializer =
            {
                new ItemsControl<IRootViewModel, ICommandTimeStateViewModel>
                    {
                        Orientation = Orientation.Horizontal,
                        ItemTemplate = () =>
                        {
                            var grid = new Grid<ICommandTimeStateViewModel>
                            {
                                Margin = "0 0 1 0",
                                Width = 20,
                                RowDefinitionsObject = "Auto Auto",
                                ChildInitializer =
                                {
                                    new Grid<ICommandTimeStateViewModel>
                                    {
                                        ColumnDefinitionsObject = "* Auto",
                                        ChildInitializer =
                                        {
                                            new TextBlock<ICommandTimeStateViewModel>().Setup(t => t.Bind(
                                                t,
                                                dc => dc.DisplayLabel.Value,
                                                t => t.Text)),
                                            new TextBlock<ICommandTimeStateViewModel>
                                            {
                                                Width = 5,
                                                TextAlignment = TextAlignment.Right,
                                                Extensions = {new GridPositionExtension(1, 0)}
                                            }.Setup(t => t.Bind(
                                                t,
                                                dc => dc.TotalProgress.Value,
                                                t => t.Text,
                                                v => $"{v}%")),
                                        }
                                    },
                                    new ProgressBar<ICommandTimeStateViewModel>
                                        {
                                            Theme = new ProgressBarTheme
                                            {
                                                ForegroundColor = _progressBarTheme?.ForegroundColor,
                                                UnfilledForeground = _progressBarTheme?.UnfilledForeground,
                                                FilledCharacter = '\u2594',
                                                UnfilledCharacter = '\u2594',
                                                Fraction1Per8Character = '\u2594',
                                                Fraction2Per8Character = '\u2594',
                                                Fraction3Per8Character = '\u2594',
                                                Fraction4Per8Character = '\u2594',
                                                Fraction5Per8Character = '\u2594',
                                                Fraction6Per8Character = '\u2594',
                                                Fraction7Per8Character = '\u2594',
                                                FractionFull = '\u2594',
                                            },
                                            Extensions =
                                            {
                                                new GridPositionExtension(0, 1)
                                            }
                                        }
                                        .Setup(p => p.Bind(
                                            p,
                                            dc => dc.TotalProgress.Value,
                                            p => p.Value)),
                                }
                            };

                            return grid;
                        }
                    }
                    .Setup(i => i.Bind(
                        i,
                        dc => dc.TimelineViewModel.ParallelCommandsGroups[0].Commands,
                        i => i.ItemsSource,
                        v => v))
            }
        };

        return root;
    }
}