using PropertyChanged.SourceGenerator;
using TerminalUI.Color;
using TerminalUI.Extensions;
using TerminalUI.Models;

namespace TerminalUI.Controls;

public partial class TextBlock<T> : View<T>
{
    private record RenderContext(Position Position, string? Text, IColor? Foreground, IColor? Background);

    private RenderContext? _renderContext;

    [Notify] private string? _text = string.Empty;
    [Notify] private IColor? _foreground;
    [Notify] private IColor? _background;

    public TextBlock()
    {
        this.Bind(
            this,
            dc => dc == null ? string.Empty : dc.ToString(),
            tb => tb.Text
        );

        RerenderProperties.Add(nameof(Text));
        RerenderProperties.Add(nameof(Foreground));
        RerenderProperties.Add(nameof(Background));
    }

    public override Size GetRequestedSize() => new(Text?.Length ?? 0, 1);

    protected override void DefaultRenderer(Position position, Size size)
    {
        var driver = ApplicationContext!.ConsoleDriver;
        var renderContext = new RenderContext(position, Text, _foreground, _background);
        if (!NeedsRerender(renderContext)) return;

        _renderContext = renderContext;

        if (Text is null) return;

        driver.SetCursorPosition(position);
        driver.ResetColor();
        if (Foreground is { } foreground)
        {
            driver.SetForegroundColor(foreground);
        }

        if (Background is { } background)
        {
            driver.SetBackgroundColor(background);
        }

        driver.Write(Text);
    }

    private bool NeedsRerender(RenderContext renderContext)
        => _renderContext is null || _renderContext != renderContext;
}