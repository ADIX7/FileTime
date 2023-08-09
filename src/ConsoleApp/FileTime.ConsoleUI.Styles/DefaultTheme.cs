using FileTime.ConsoleUI.App;
using TerminalUI.Color;

namespace FileTime.ConsoleUI.Styles;

public record Theme(
    IColor? DefaultForegroundColor,
    IColor? DefaultBackgroundColor,
    IColor? ElementColor,
    IColor? ContainerColor,
    IColor? MarkedItemColor) : ITheme;

public static class DefaultThemes
{
    public static Theme Color256Theme => new(
        DefaultForegroundColor: Color256Colors.Foregrounds.Gray,
        DefaultBackgroundColor: Color256Colors.Foregrounds.Black,
        ElementColor: Color256Colors.Foregrounds.Gray,
        ContainerColor: Color256Colors.Foregrounds.Blue,
        MarkedItemColor: Color256Colors.Foregrounds.Black
    );

    public static Theme ConsoleColorTheme => new(
        DefaultForegroundColor: ConsoleColors.Foregrounds.Gray,
        DefaultBackgroundColor: ConsoleColors.Foregrounds.Black,
        ElementColor: ConsoleColors.Foregrounds.Gray,
        ContainerColor: ConsoleColors.Foregrounds.Blue,
        MarkedItemColor: ConsoleColors.Foregrounds.Black);
}