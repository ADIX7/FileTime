using TerminalUI.Models;

namespace FileTime.ConsoleUI.App.Configuration.Theme;

public class ProgressBarThemeConfiguration
{
    public string? ForegroundColor { get; set; }
    public string? BackgroundColor { get; set; }
    public string? UnfilledForeground { get; set; }
    public string? UnfilledBackground { get; set; }
    public SelectiveChar? FilledCharacter { get; set; }
    public SelectiveChar? UnfilledCharacter { get; set; }

    public SelectiveChar? Fraction1Per8Character { get; set; }
    public SelectiveChar? Fraction2Per8Character { get; set; }
    public SelectiveChar? Fraction3Per8Character { get; set; }
    public SelectiveChar? Fraction4Per8Character { get; set; }
    public SelectiveChar? Fraction5Per8Character { get; set; }
    public SelectiveChar? Fraction6Per8Character { get; set; }
    public SelectiveChar? Fraction7Per8Character { get; set; }
    public SelectiveChar? FractionFull { get; set; }
    public SelectiveChar? LeftCap { get; set; }
    public SelectiveChar? RightCap { get; set; }
}