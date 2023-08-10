using System.ComponentModel;
using PropertyChanged.SourceGenerator;
using TerminalUI.Color;
using TerminalUI.ConsoleDrivers;
using TerminalUI.Extensions;
using TerminalUI.Models;

namespace TerminalUI.Controls;

public partial class TextBlock<T> : View<T>
{
    private record RenderState(
        Position Position,
        Size Size,
        string? Text,
        IColor? Foreground,
        IColor? Background);

    private RenderState? _lastRenderState;
    private string[]? _textLines;
    private bool _placeholderRenderDone;

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

        ((INotifyPropertyChanged) this).PropertyChanged += (o, e) =>
        {
            if (e.PropertyName == nameof(Text))
            {
                _textLines = Text?.Split(Environment.NewLine);
            }
        };
    }

    protected override Size CalculateSize() => new(_textLines?.Max(l => l.Length) ?? 0, _textLines?.Length ?? 0);

    protected override bool DefaultRenderer(RenderContext renderContext, Position position, Size size)
    {
        if (size.Width == 0 || size.Height == 0) return false;

        var driver = renderContext.ConsoleDriver;
        var renderState = new RenderState(position, size, Text, _foreground, _background);
        if (!NeedsRerender(renderState)) return false;

        _lastRenderState = renderState;

        if (_textLines is null)
        {
            if (_placeholderRenderDone)
            {
                _placeholderRenderDone = true;
                RenderEmpty(renderContext, position, size);
            }

            return false;
        }

        _placeholderRenderDone = false;

        driver.ResetColor();
        if (Foreground is { } foreground)
        {
            driver.SetForegroundColor(foreground);
        }

        if (Background is { } background)
        {
            driver.SetBackgroundColor(background);
        }

        RenderText(_textLines, driver, position, size);

        return true;
    }

    private void RenderText(string[] textLines, IConsoleDriver driver, Position position, Size size)
    {
        for (var i = 0; i < textLines.Length; i++)
        {
            var text = textLines[i];
            text = TextAlignment switch
            {
                TextAlignment.Right => string.Format($"{{0,{size.Width}}}", text),
                _ => string.Format($"{{0,{-size.Width}}}", text)
            };
            if (text.Length > size.Width)
            {
                text = text[..size.Width];
            }

            driver.SetCursorPosition(position with {Y = position.Y + i});
            driver.Write(text);
        }
    }

    private bool NeedsRerender(RenderState renderState)
        => _lastRenderState is null || _lastRenderState != renderState;
}