using TerminalUI.Color;
using TerminalUI.Models;

namespace TerminalUI.Styling.Controls;

public interface IProgressBarTheme
{
    IColor? ForegroundColor { get; init; }
    IColor? BackgroundColor { get; init; }
    IColor? UnfilledForeground { get; init; }
    IColor? UnfilledBackground { get; init; }
    SelectiveChar? FilledCharacter { get; init; }
    SelectiveChar? UnfilledCharacter { get; init; }
    SelectiveChar? Fraction1Per8Character { get; init; }
    SelectiveChar? Fraction2Per8Character { get; init; }
    SelectiveChar? Fraction3Per8Character { get; init; }
    SelectiveChar? Fraction4Per8Character { get; init; }
    SelectiveChar? Fraction5Per8Character { get; init; }
    SelectiveChar? Fraction6Per8Character { get; init; }
    SelectiveChar? Fraction7Per8Character { get; init; }
    SelectiveChar? FractionFull { get; init; }
    SelectiveChar? LeftCap { get; init; }
    SelectiveChar? RightCap { get; init; }
}