using FileTime.ConsoleUI.App;
using FileTime.ConsoleUI.App.Styling;
using TerminalUI.Color;
using TerminalUI.Styling;
using TerminalUI.Styling.Controls;
using ITheme = FileTime.ConsoleUI.App.Styling.ITheme;

namespace FileTime.ConsoleUI.Styles;

using IConsoleTheme = TerminalUI.Styling.ITheme;
using ConsoleTheme = TerminalUI.Styling.Theme;

public record Theme(
    IColor? DefaultForegroundColor,
    IColor? DefaultForegroundAccentColor,
    IColor? DefaultBackgroundColor,
    IColor? ElementColor,
    IColor? ContainerColor,
    IColor? MarkedItemForegroundColor,
    IColor? MarkedItemBackgroundColor,
    IColor? MarkedSelectedItemForegroundColor,
    IColor? MarkedSelectedItemBackgroundColor,
    IColor? SelectedItemColor,
    IColor? SelectedTabBackgroundColor,
    IColor? WarningForegroundColor,
    IColor? ErrorForegroundColor,
    ListViewItemTheme ListViewItemTheme,
    IConsoleTheme? ConsoleTheme,
    Type? ForegroundColors,
    Type? BackgroundColors) : ITheme, IColorSampleProvider;

public static class DefaultThemes
{
    public static Theme Color256Theme => new(
        DefaultForegroundColor: null,
        DefaultForegroundAccentColor: Color256Colors.Foregrounds.Red,
        DefaultBackgroundColor: null,
        ElementColor: Color256Colors.Foregrounds.Gray,
        ContainerColor: Color256Colors.Foregrounds.Blue,
        MarkedItemForegroundColor: Color256Colors.Foregrounds.Yellow,
        MarkedItemBackgroundColor: null,
        MarkedSelectedItemForegroundColor: Color256Colors.Foregrounds.Black,
        MarkedSelectedItemBackgroundColor: Color256Colors.Foregrounds.Yellow,
        SelectedItemColor: Color256Colors.Foregrounds.Black,
        SelectedTabBackgroundColor: Color256Colors.Backgrounds.Green,
        WarningForegroundColor: Color256Colors.Foregrounds.Yellow,
        ErrorForegroundColor: Color256Colors.Foregrounds.Red,
        ListViewItemTheme: new(
            SelectedBackgroundColor: Color256Colors.Backgrounds.Gray,
            SelectedForegroundColor: Color256Colors.Foregrounds.Black
        ),
        ConsoleTheme: new ConsoleTheme
        {
            ControlThemes = new ControlThemes
            {
                ProgressBar = new ProgressBarTheme
                {
                    ForegroundColor = Color256Colors.Foregrounds.Blue,
                    BackgroundColor = Color256Colors.Backgrounds.Gray,
                    UnfilledForeground = Color256Colors.Foregrounds.Gray,
                    UnfilledBackground = Color256Colors.Backgrounds.Gray,
                }
            }
        },
        ForegroundColors: typeof(Color256Colors.Foregrounds),
        BackgroundColors: typeof(Color256Colors.Backgrounds)
    );

    public static Theme ConsoleColorTheme => new(
        DefaultForegroundColor: null,
        DefaultForegroundAccentColor: ConsoleColors.Foregrounds.Red,
        DefaultBackgroundColor: null,
        ElementColor: ConsoleColors.Foregrounds.Gray,
        ContainerColor: ConsoleColors.Foregrounds.Blue,
        MarkedItemForegroundColor: ConsoleColors.Foregrounds.Yellow,
        MarkedItemBackgroundColor: null,
        MarkedSelectedItemForegroundColor: ConsoleColors.Foregrounds.Black,
        MarkedSelectedItemBackgroundColor: ConsoleColors.Foregrounds.Yellow,
        SelectedItemColor: ConsoleColors.Foregrounds.Black,
        SelectedTabBackgroundColor: ConsoleColors.Backgrounds.Green,
        WarningForegroundColor: ConsoleColors.Foregrounds.Yellow,
        ErrorForegroundColor: ConsoleColors.Foregrounds.Red,
        ListViewItemTheme: new(
            SelectedBackgroundColor: ConsoleColors.Backgrounds.Gray,
            SelectedForegroundColor: ConsoleColors.Foregrounds.Black
        ),
        ConsoleTheme: new ConsoleTheme
        {
            ControlThemes = new ControlThemes
            {
                ProgressBar = new ProgressBarTheme
                {
                    ForegroundColor = ConsoleColors.Foregrounds.Blue,
                    BackgroundColor = ConsoleColors.Backgrounds.Gray,
                    UnfilledForeground = ConsoleColors.Foregrounds.Gray,
                    UnfilledBackground = ConsoleColors.Backgrounds.Gray
                }
            }
        },
        ForegroundColors: typeof(ConsoleColors.Foregrounds),
        BackgroundColors: typeof(ConsoleColors.Backgrounds)
    );
}