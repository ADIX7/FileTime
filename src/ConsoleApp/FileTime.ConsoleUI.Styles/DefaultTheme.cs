using FileTime.ConsoleUI.App;
using TerminalUI.Color;

namespace FileTime.ConsoleUI.Styles;

public record Theme(
    IColor? DefaultForegroundColor,
    IColor? DefaultBackgroundColor,
    IColor? ElementColor,
    IColor? ContainerColor,
    IColor? MarkedItemColor,
    IColor? SelectedTabBackgroundColor,
    Type? ForegroundColors,
    Type? BackgroundColors) : ITheme, IColorSampleProvider;

public static class DefaultThemes
{
    public static Theme Color256Theme => new(
        DefaultForegroundColor: Color256Colors.Foregrounds.Gray,
        DefaultBackgroundColor: null,
        ElementColor: Color256Colors.Foregrounds.Gray,
        ContainerColor: Color256Colors.Foregrounds.Blue,
        MarkedItemColor: Color256Colors.Foregrounds.Black,
        SelectedTabBackgroundColor: Color256Colors.Backgrounds.Green,
        ForegroundColors: typeof(Color256Colors.Foregrounds),
        BackgroundColors: typeof(Color256Colors.Backgrounds)
    );

    public static Theme ConsoleColorTheme => new(
        DefaultForegroundColor: ConsoleColors.Foregrounds.Gray,
        DefaultBackgroundColor: ConsoleColors.Backgrounds.Black,
        ElementColor: ConsoleColors.Foregrounds.Gray,
        ContainerColor: ConsoleColors.Foregrounds.Blue,
        MarkedItemColor: ConsoleColors.Foregrounds.Black,
        SelectedTabBackgroundColor: ConsoleColors.Backgrounds.Green,
        ForegroundColors: typeof(ConsoleColors.Foregrounds),
        BackgroundColors: typeof(ConsoleColors.Backgrounds)
    );
}