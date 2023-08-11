using FileTime.ConsoleUI.App;
using FileTime.ConsoleUI.App.Styling;
using TerminalUI.Color;

namespace FileTime.ConsoleUI.Styles;

public record Theme(
    IColor? DefaultForegroundColor,
    IColor? DefaultBackgroundColor,
    IColor? ElementColor,
    IColor? ContainerColor,
    IColor? MarkedItemForegroundColor,
    IColor? MarkedItemBackgroundColor,
    IColor? MarkedSelectedItemForegroundColor,
    IColor? MarkedSelectedItemBackgroundColor,
    IColor? SelectedItemColor,
    IColor? SelectedTabBackgroundColor,
    ListViewItemTheme ListViewItemTheme,
    Type? ForegroundColors,
    Type? BackgroundColors) : ITheme, IColorSampleProvider;

public static class DefaultThemes
{
    public static Theme Color256Theme => new(
        DefaultForegroundColor: Color256Colors.Foregrounds.Gray,
        DefaultBackgroundColor: null,
        ElementColor: Color256Colors.Foregrounds.Gray,
        ContainerColor: Color256Colors.Foregrounds.Blue,
        MarkedItemForegroundColor: Color256Colors.Foregrounds.Yellow,
        MarkedItemBackgroundColor: null,
        MarkedSelectedItemForegroundColor: Color256Colors.Foregrounds.Black,
        MarkedSelectedItemBackgroundColor: Color256Colors.Foregrounds.Yellow,
        SelectedItemColor: Color256Colors.Foregrounds.Black,
        SelectedTabBackgroundColor: Color256Colors.Backgrounds.Green,
        ListViewItemTheme: new(
            SelectedBackgroundColor: Color256Colors.Backgrounds.Gray,
            SelectedForegroundColor: Color256Colors.Foregrounds.Black
        ),
        ForegroundColors: typeof(Color256Colors.Foregrounds),
        BackgroundColors: typeof(Color256Colors.Backgrounds)
    );

    public static Theme ConsoleColorTheme => new(
        DefaultForegroundColor: ConsoleColors.Foregrounds.Gray,
        DefaultBackgroundColor: null,
        ElementColor: ConsoleColors.Foregrounds.Gray,
        ContainerColor: ConsoleColors.Foregrounds.Blue,
        MarkedItemForegroundColor: ConsoleColors.Foregrounds.Yellow,
        MarkedItemBackgroundColor: null,
        MarkedSelectedItemForegroundColor: ConsoleColors.Foregrounds.Black,
        MarkedSelectedItemBackgroundColor: ConsoleColors.Foregrounds.Yellow,
        SelectedItemColor: ConsoleColors.Foregrounds.Black,
        SelectedTabBackgroundColor: ConsoleColors.Backgrounds.Green,
        ListViewItemTheme: new(
            SelectedBackgroundColor: ConsoleColors.Backgrounds.Gray,
            SelectedForegroundColor: ConsoleColors.Foregrounds.Black
        ),
        ForegroundColors: typeof(ConsoleColors.Foregrounds),
        BackgroundColors: typeof(ConsoleColors.Backgrounds)
    );
}