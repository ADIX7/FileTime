using PropertyChanged.SourceGenerator;
using TerminalUI.Color;
using TerminalUI.Models;
using TerminalUI.Styling.Controls;

namespace TerminalUI.Controls;

public partial class ProgressBar<T> : View<ProgressBar<T>, T>
{
    private record RenderState(
        Position Position,
        Size Size,
        int Minimum,
        int Maximum,
        int Value,
        char? LeftCap,
        char? RightCap,
        char? Fill,
        char? Unfilled,
        IColor? UnfilledForeground,
        IColor? UnfilledBackground);

    private RenderState? _lastRenderState;

    [Notify] private int _minimum = 0;
    [Notify] private int _maximum = 100;
    [Notify] private int _value = 0;
    [Notify] private IProgressBarTheme? _theme;

    private IProgressBarTheme? AppTheme => ApplicationContext?.Theme?.ControlThemes.ProgressBar;

    public ProgressBar()
    {
        RerenderProperties.Add(nameof(Minimum));
        RerenderProperties.Add(nameof(Maximum));
        RerenderProperties.Add(nameof(Value));
        RerenderProperties.Add(nameof(Theme));
    }


    protected override Size CalculateSize() => new(5, 1);

    protected override bool DefaultRenderer(in RenderContext renderContext, Position position, Size size)
    {
        if (size.Width == 0 || size.Height == 0) return false;
        var theme = AppTheme;

        var foreground = Foreground ?? (_theme ?? theme)?.ForegroundColor ?? renderContext.Foreground;
        var background = Background ?? (_theme ?? theme)?.BackgroundColor ?? renderContext.Background;
        var unfilledForeground = (_theme ?? theme)?.UnfilledForeground ?? renderContext.Foreground;
        var unfilledBackground = (_theme ?? theme)?.UnfilledBackground ?? renderContext.Background;
        var unfilledCharacter = (_theme ?? theme)?.UnfilledCharacter ?? ApplicationContext?.EmptyCharacter ?? ' ';
        var fillCharacter = (_theme ?? theme)?.FilledCharacter ?? '█';
        var leftCap = (_theme ?? theme)?.LeftCap;
        var rightCap = (_theme ?? theme)?.RightCap;

        var renderState = new RenderState(
            position,
            size,
            Minimum,
            Maximum,
            Value,
            leftCap,
            rightCap,
            fillCharacter,
            unfilledCharacter,
            unfilledForeground,
            unfilledBackground);

        if (!renderContext.ForceRerender && !NeedsRerender(renderState)) return false;

        _lastRenderState = renderState;
        var driver = renderContext.ConsoleDriver;

        var borderWidth =
            (leftCap.HasValue ? 1 : 0)
            + (rightCap.HasValue ? 1 : 0);

        var progress = (double) (Value - Minimum) / (Maximum - Minimum);
        var progressAvailableSpace = size.Width - borderWidth;
        var progressWidth = progress * progressAvailableSpace;
        var progressQuotientWidth = (int) Math.Floor(progressWidth);
        var progressRemainderWidth = progressAvailableSpace - progressQuotientWidth - 1;
        if (progressRemainderWidth < 0) progressRemainderWidth = 0;

        Span<char> filledText = stackalloc char[progressQuotientWidth];
        var transientChar = unfilledCharacter;

        filledText.Fill(fillCharacter);
        if (ApplicationContext!.SupportUtf8Output)
        {
            var remained = progressWidth - progressQuotientWidth;
            transientChar = remained switch
            {
                < 0.125 => unfilledCharacter,
                < 0.250 => (_theme ?? theme)?.Fraction1Per8Character ?? '\u258F',
                < 0.375 => (_theme ?? theme)?.Fraction2Per8Character ?? '\u258E',
                < 0.500 => (_theme ?? theme)?.Fraction3Per8Character ?? '\u258D',
                < 0.675 => (_theme ?? theme)?.Fraction4Per8Character ?? '\u258C',
                < 0.750 => (_theme ?? theme)?.Fraction5Per8Character ?? '\u258B',
                < 0.875 => (_theme ?? theme)?.Fraction6Per8Character ?? '\u258A',
                < 0_001 => (_theme ?? theme)?.Fraction7Per8Character ?? '\u2589',
                _ => (_theme ?? theme)?.FractionFull ?? '\u2588',
            };
        }

        SetColor(driver, foreground, background);

        // Left border
        var textStartPosition = position;
        if (leftCap.HasValue)
        {
            RenderText(leftCap.Value, driver, position, size with {Width = 1});
            textStartPosition = textStartPosition with {X = textStartPosition.X + 1};
        }

        // Filled
        RenderText(
            filledText,
            driver,
            textStartPosition,
            size with {Width = progressQuotientWidth}
        );

        // Transient character
        if (progressQuotientWidth < progressAvailableSpace)
        {
            SetColor(driver, foreground, unfilledBackground);
            RenderText(
                transientChar,
                driver,
                textStartPosition with {X = textStartPosition.X + progressQuotientWidth},
                size with {Width = 1}
            );
        }

        // Unfilled
        if (progressRemainderWidth != 0)
        {
            Span<char> unfilledText = stackalloc char[progressRemainderWidth];
            unfilledText.Fill(unfilledCharacter);

            SetColor(driver, unfilledForeground, unfilledBackground);
            RenderText(
                unfilledText,
                driver,
                textStartPosition with {X = textStartPosition.X + progressQuotientWidth + 1},
                size with {Width = progressRemainderWidth}
            );
        }

        // Right border
        if (rightCap.HasValue)
        {
            SetColor(driver, foreground, background);
            RenderText(
                rightCap.Value,
                driver,
                position with {X = position.X + size.Width - 1},
                size with {Width = 1});
        }

        return true;
    }

    private bool NeedsRerender(RenderState renderState)
        => _lastRenderState is null || _lastRenderState != renderState;
}