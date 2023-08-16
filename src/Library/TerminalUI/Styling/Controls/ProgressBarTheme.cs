using TerminalUI.Color;
using TerminalUI.Models;

namespace TerminalUI.Styling.Controls;

public class ProgressBarTheme : IProgressBarTheme
{
    public ProgressBarTheme(){}
    public ProgressBarTheme(ProgressBarTheme theme)
    {
        ForegroundColor = theme.ForegroundColor;
        BackgroundColor = theme.BackgroundColor;
        UnfilledForeground = theme.UnfilledForeground;
        UnfilledBackground = theme.UnfilledBackground;
        FilledCharacter = theme.FilledCharacter;
        UnfilledCharacter = theme.UnfilledCharacter;
        Fraction1Per8Character = theme.Fraction1Per8Character;
        Fraction2Per8Character = theme.Fraction2Per8Character;
        Fraction3Per8Character = theme.Fraction3Per8Character;
        Fraction4Per8Character = theme.Fraction4Per8Character;
        Fraction5Per8Character = theme.Fraction5Per8Character;
        Fraction6Per8Character = theme.Fraction6Per8Character;
        Fraction7Per8Character = theme.Fraction7Per8Character;
        FractionFull = theme.FractionFull;
        LeftCap = theme.LeftCap;
        RightCap = theme.RightCap;
    }
    public IColor? ForegroundColor { get; init; }
    public IColor? BackgroundColor { get; init; }
    public IColor? UnfilledForeground { get; init; }
    public IColor? UnfilledBackground { get; init; }
    public SelectiveChar? FilledCharacter { get; init; }
    public SelectiveChar? UnfilledCharacter { get; init; }
    public SelectiveChar? Fraction1Per8Character { get; init; }
    public SelectiveChar? Fraction2Per8Character { get; init; }
    public SelectiveChar? Fraction3Per8Character { get; init; }
    public SelectiveChar? Fraction4Per8Character { get; init; }
    public SelectiveChar? Fraction5Per8Character { get; init; }
    public SelectiveChar? Fraction6Per8Character { get; init; }
    public SelectiveChar? Fraction7Per8Character { get; init; }
    public SelectiveChar? FractionFull { get; init; }
    public SelectiveChar? LeftCap { get; init; }
    public SelectiveChar? RightCap { get; init; }
}