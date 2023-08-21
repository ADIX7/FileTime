using FileTime.ConsoleUI.App.Configuration;
using FileTime.ConsoleUI.App.Configuration.Theme;
using FileTime.ConsoleUI.App.Styling;
using Microsoft.Extensions.Options;
using PropertyChanged.SourceGenerator;
using TerminalUI.Color;
using TerminalUI.Styling;
using TerminalUI.Styling.Controls;
using IConsoleTheme = TerminalUI.Styling.ITheme;
using ConsoleTheme = TerminalUI.Styling.Theme;
using ITheme = FileTime.ConsoleUI.App.Styling.ITheme;
using Theme = FileTime.ConsoleUI.App.Styling.Theme;

namespace FileTime.ConsoleUI.App.Services;

public partial class ThemeProvider : IThemeProvider
{
    private readonly ITheme _defaultTheme;
    private readonly IColorProvider _colorProvider;
    private readonly IOptionsMonitor<StyleConfigurationRoot> _styleConfiguration;

    [Notify] private ITheme _currentTheme = null!;

    public ThemeProvider(
        ITheme defaultTheme,
        IColorProvider colorProvider,
        IOptionsMonitor<StyleConfigurationRoot> styleConfiguration
    )
    {
        _defaultTheme = defaultTheme;
        _colorProvider = colorProvider;
        _styleConfiguration = styleConfiguration;
        styleConfiguration.OnChange(ThemeConfigurationChanged);
        UpdateCurrentTheme();
    }

    private void ThemeConfigurationChanged(StyleConfigurationRoot arg1, string? arg2) => UpdateCurrentTheme();

    private void UpdateCurrentTheme()
    {
        var currentThemeName = _styleConfiguration.CurrentValue.Theme;
        ThemeConfiguration? currentTheme = null;
        if (currentThemeName is not null
            && _styleConfiguration.CurrentValue.Themes.TryGetValue(currentThemeName, out currentTheme)
            || currentTheme is null)
        {
            CurrentTheme = _defaultTheme;
            return;
        }

        var theme = new Theme(
            ParseColor(currentTheme.DefaultForegroundColor) ?? _defaultTheme.DefaultForegroundColor,
            ParseColor(currentTheme.DefaultForegroundAccentColor) ?? _defaultTheme.DefaultForegroundAccentColor,
            ParseColor(currentTheme.DefaultBackgroundColor) ?? _defaultTheme.DefaultBackgroundColor,
            ParseColor(currentTheme.ElementColor) ?? _defaultTheme.ElementColor,
            ParseColor(currentTheme.ContainerColor) ?? _defaultTheme.ContainerColor,
            ParseColor(currentTheme.MarkedItemForegroundColor) ?? _defaultTheme.MarkedItemForegroundColor,
            ParseColor(currentTheme.MarkedItemBackgroundColor) ?? _defaultTheme.MarkedItemBackgroundColor,
            ParseColor(currentTheme.MarkedSelectedItemForegroundColor) ?? _defaultTheme.MarkedSelectedItemForegroundColor,
            ParseColor(currentTheme.MarkedSelectedItemBackgroundColor) ?? _defaultTheme.MarkedSelectedItemBackgroundColor,
            ParseColor(currentTheme.SelectedItemColor) ?? _defaultTheme.SelectedItemColor,
            ParseColor(currentTheme.SelectedTabBackgroundColor) ?? _defaultTheme.SelectedTabBackgroundColor,
            ParseColor(currentTheme.WarningForegroundColor) ?? _defaultTheme.WarningForegroundColor,
            ParseColor(currentTheme.ErrorForegroundColor) ?? _defaultTheme.ErrorForegroundColor,
            CreateListViewItemTheme(currentTheme.ListViewItemTheme),
            CreateConsoleTheme(currentTheme.ConsoleTheme)
        );

        CurrentTheme = theme;
    }

    private IColor? ParseColor(string? colorString, bool foreground = true)
        => colorString is null
            ? null
            : _colorProvider.Parse(colorString, foreground ? ColorType.Foreground : ColorType.Background);

    private ListViewItemTheme CreateListViewItemTheme(ListViewItemThemeConfiguration? currentThemeListViewItemTheme)
    {
        var theme = new ListViewItemTheme(
            ParseColor(currentThemeListViewItemTheme?.SelectedForegroundColor) ?? _defaultTheme.ListViewItemTheme.SelectedForegroundColor,
            ParseColor(currentThemeListViewItemTheme?.SelectedBackgroundColor) ?? _defaultTheme.ListViewItemTheme.SelectedBackgroundColor
        );

        return theme;
    }

    private IConsoleTheme CreateConsoleTheme(ConsoleThemeConfiguration? currentThemeConsoleTheme)
    {
        var controlThemes = currentThemeConsoleTheme?.ControlThemes;
        var progressBarTheme = controlThemes?.ProgressBar;

        var defaultControlThemes = _defaultTheme.ConsoleTheme?.ControlThemes;
        var defaultProgressBarTheme = defaultControlThemes?.ProgressBar;

        var theme = new ConsoleTheme
        {
            ControlThemes = new ControlThemes
            {
                ProgressBar = new ProgressBarTheme
                {
                    ForegroundColor = ParseColor(progressBarTheme?.ForegroundColor) ?? defaultProgressBarTheme?.ForegroundColor,
                    BackgroundColor = ParseColor(progressBarTheme?.BackgroundColor) ?? defaultProgressBarTheme?.BackgroundColor,
                    UnfilledForeground = ParseColor(progressBarTheme?.UnfilledForeground) ?? defaultProgressBarTheme?.UnfilledForeground,
                    UnfilledBackground = ParseColor(progressBarTheme?.UnfilledBackground) ?? defaultProgressBarTheme?.UnfilledBackground,
                    FilledCharacter = progressBarTheme?.FilledCharacter ?? defaultProgressBarTheme?.FilledCharacter,
                    UnfilledCharacter = progressBarTheme?.UnfilledCharacter ?? defaultProgressBarTheme?.UnfilledCharacter,
                    Fraction1Per8Character = progressBarTheme?.Fraction1Per8Character ?? defaultProgressBarTheme?.Fraction1Per8Character,
                    Fraction2Per8Character = progressBarTheme?.Fraction2Per8Character ?? defaultProgressBarTheme?.Fraction2Per8Character,
                    Fraction3Per8Character = progressBarTheme?.Fraction3Per8Character ?? defaultProgressBarTheme?.Fraction3Per8Character,
                    Fraction4Per8Character = progressBarTheme?.Fraction4Per8Character ?? defaultProgressBarTheme?.Fraction4Per8Character,
                    Fraction5Per8Character = progressBarTheme?.Fraction5Per8Character ?? defaultProgressBarTheme?.Fraction5Per8Character,
                    Fraction6Per8Character = progressBarTheme?.Fraction6Per8Character ?? defaultProgressBarTheme?.Fraction6Per8Character,
                    Fraction7Per8Character = progressBarTheme?.Fraction7Per8Character ?? defaultProgressBarTheme?.Fraction7Per8Character,
                    FractionFull = progressBarTheme?.FractionFull ?? defaultProgressBarTheme?.FractionFull,
                    LeftCap = progressBarTheme?.LeftCap ?? defaultProgressBarTheme?.LeftCap,
                    RightCap = progressBarTheme?.RightCap ?? defaultProgressBarTheme?.RightCap,
                }
            }
        };

        return theme;
    }
}