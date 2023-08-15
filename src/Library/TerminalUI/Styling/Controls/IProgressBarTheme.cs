using TerminalUI.Color;

namespace TerminalUI.Styling.Controls;

public interface IProgressBarTheme
{
    IColor? ForegroundColor { get; init; }
    IColor? BackgroundColor { get; init; }
    IColor? UnfilledForeground { get; init; }
    IColor? UnfilledBackground { get; init; }
    char? FilledCharacter { get; init; }
    char? UnfilledCharacter { get; init; }
    char? Fraction1Per8Character { get; init; }
    char? Fraction2Per8Character { get; init; }
    char? Fraction3Per8Character { get; init; }
    char? Fraction4Per8Character { get; init; }
    char? Fraction5Per8Character { get; init; }
    char? Fraction6Per8Character { get; init; }
    char? Fraction7Per8Character { get; init; }
    char? FractionFull { get; init; }
    char? LeftCap { get; init; }
    char? RightCap { get; init; }
}