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
    [Notify] private TextAlignment _textAlignment = TextAlignment.Left;

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
        RerenderProperties.Add(nameof(TextAlignment));
    }

    public override Size GetRequestedSize() => new(Text?.Length ?? 0, 1);

    protected override void DefaultRenderer(Position position, Size size)
    {
        if (size.Width == 0 || size.Height == 0) return;

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

        var text = TextAlignment switch
        {
            TextAlignment.Right => string.Format($"{{0,{size.Width}}}", Text),
            _ => string.Format($"{{0,{-size.Width}}}", Text)
        };
        if (text.Length > size.Width)
        {
            text = text[..size.Width];
        }

        driver.Write(text);
    }

    private bool NeedsRerender(RenderContext renderContext)
        => _renderContext is null || _renderContext != renderContext;
}