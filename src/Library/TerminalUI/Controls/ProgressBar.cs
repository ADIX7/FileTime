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
        SelectiveChar? LeftCap,
        SelectiveChar? RightCap,
        SelectiveChar? Fill,
        SelectiveChar? Unfilled,
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
        var unfilledCharacterS = (_theme ?? theme)?.UnfilledCharacter ?? new SelectiveChar(ApplicationContext?.EmptyCharacter ?? ' ');
        var fillCharacterS = (_theme ?? theme)?.FilledCharacter ?? new SelectiveChar('█');
        var leftCapS = (_theme ?? theme)?.LeftCap;
        var rightCapS = (_theme ?? theme)?.RightCap;

        var renderState = new RenderState(
            position,
            size,
            Minimum,
            Maximum,
            Value,
            leftCapS,
            rightCapS,
            fillCharacterS,
            unfilledCharacterS,
            unfilledForeground,
            unfilledBackground);

        if (!renderContext.ForceRerender && !NeedsRerender(renderState)) return false;

        var utf8Support = ApplicationContext!.SupportUtf8Output;
        var unfilledCharacter = unfilledCharacterS.GetChar(utf8Support);
        var fillCharacter = fillCharacterS.GetChar(utf8Support);
        var leftCap = leftCapS?.GetChar(utf8Support);
        var rightCap = rightCapS?.GetChar(utf8Support);

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
            var t = _theme ?? theme;
            transientChar = remained switch
            {
                < 0.125 => unfilledCharacter,
                < 0.250 => (t?.Fraction1Per8Character ?? new SelectiveChar('\u258F', ' ')).GetChar(utf8Support),
                < 0.375 => (t?.Fraction2Per8Character ?? new SelectiveChar('\u258E', ' ')).GetChar(utf8Support),
                < 0.500 => (t?.Fraction3Per8Character ?? new SelectiveChar('\u258D', ' ')).GetChar(utf8Support),
                < 0.675 => (t?.Fraction4Per8Character ?? new SelectiveChar('\u258C', ' ')).GetChar(utf8Support),
                < 0.750 => (t?.Fraction5Per8Character ?? new SelectiveChar('\u258B', ' ')).GetChar(utf8Support),
                < 0.875 => (t?.Fraction6Per8Character ?? new SelectiveChar('\u258A', ' ')).GetChar(utf8Support),
                < 0_001 => (t?.Fraction7Per8Character ?? new SelectiveChar('\u2589', ' ')).GetChar(utf8Support),
                _ => (t?.FractionFull ?? new SelectiveChar('\u2588', ' ')).GetChar(utf8Support),
            };
        }

        SetStyleColor(renderContext, foreground, background);

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
            SetStyleColor(renderContext, foreground, unfilledBackground);
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

            SetStyleColor(renderContext, unfilledForeground, unfilledBackground);
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
            SetStyleColor(renderContext, foreground, background);
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