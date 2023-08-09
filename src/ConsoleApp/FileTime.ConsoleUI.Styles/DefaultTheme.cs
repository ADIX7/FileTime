using FileTime.ConsoleUI.App;
using TerminalUI.Color;
using TerminalUI.Models;
using ConsoleColor = TerminalUI.Color.ConsoleColor;

namespace FileTime.ConsoleUI.Styles;

public record Theme(
    IColor? ItemBackgroundColor,
    IColor? AlternativeItemBackgroundColor,
    IColor? SelectedItemBackgroundColor,
    IColor? MarkedItemBackgroundColor,
    IColor? MarkedAlternativeItemBackgroundColor,
    IColor? MarkedSelectedItemBackgroundColor,
    IColor? DefaultForegroundColor,
    IColor? DefaultBackgroundColor,
    IColor? AlternativeItemForegroundColor,
    IColor? SelectedItemForegroundColor,
    IColor? MarkedItemForegroundColor,
    IColor? MarkedAlternativeItemForegroundColor,
    IColor? MarkedSelectedItemForegroundColor) : ITheme;

public static class DefaultThemes
{
    public static Theme Color256Theme => new(
        ItemBackgroundColor: Color256Colors.Backgrounds.Black,
        AlternativeItemBackgroundColor: Color256Colors.Backgrounds.Black,
        SelectedItemBackgroundColor: Color256Colors.Backgrounds.Black,
        MarkedItemBackgroundColor: Color256Colors.Backgrounds.Black,
        MarkedAlternativeItemBackgroundColor: Color256Colors.Backgrounds.Black,
        MarkedSelectedItemBackgroundColor: Color256Colors.Backgrounds.Black,
        DefaultForegroundColor: null,
        DefaultBackgroundColor: null,
        AlternativeItemForegroundColor: null,
        SelectedItemForegroundColor: Color256Colors.Foregrounds.Black,
        MarkedItemForegroundColor: Color256Colors.Foregrounds.White,
        MarkedAlternativeItemForegroundColor: Color256Colors.Foregrounds.White,
        MarkedSelectedItemForegroundColor: Color256Colors.Foregrounds.Cyan);

    public static Theme ConsoleColorTheme => new(
        ItemBackgroundColor: ConsoleColors.Foregrounds.Black,
        AlternativeItemBackgroundColor: ConsoleColors.Foregrounds.Black,
        SelectedItemBackgroundColor: ConsoleColors.Foregrounds.Black,
        MarkedItemBackgroundColor: ConsoleColors.Foregrounds.Black,
        MarkedAlternativeItemBackgroundColor: ConsoleColors.Foregrounds.Black,
        MarkedSelectedItemBackgroundColor: ConsoleColors.Foregrounds.Black,
        DefaultForegroundColor: null,
        DefaultBackgroundColor: null,
        AlternativeItemForegroundColor: null,
        SelectedItemForegroundColor: ConsoleColors.Foregrounds.Black,
        MarkedItemForegroundColor: ConsoleColors.Foregrounds.White,
        MarkedAlternativeItemForegroundColor: ConsoleColors.Foregrounds.White,
        MarkedSelectedItemForegroundColor: ConsoleColors.Foregrounds.Cyan);
}