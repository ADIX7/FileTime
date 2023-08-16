using System.ComponentModel;
using System.Diagnostics;
using PropertyChanged.SourceGenerator;
using TerminalUI.Color;
using TerminalUI.Models;
using TerminalUI.TextFormat;
using TerminalUI.Traits;

namespace TerminalUI.Controls;

[DebuggerDisplay("Text = {Text}")]
public sealed partial class TextBlock<T> : View<TextBlock<T>, T>, IDisplayView
{
    private record RenderState(
        Position Position,
        Size Size,
        string? Text,
        IColor? Foreground,
        IColor? Background,
        ITextFormat? TextFormat);

    private RenderState? _lastRenderState;
    private string[]? _textLines;
    private bool _placeholderRenderDone;

    [Notify] private string? _text = string.Empty;
    [Notify] private TextAlignment _textAlignment = TextAlignment.Left;
    [Notify] private ITextFormat? _textFormat;

    public TextBlock()
    {
        RerenderProperties.Add(nameof(Text));
        RerenderProperties.Add(nameof(TextAlignment));
        RerenderProperties.Add(nameof(TextFormat));

        ((INotifyPropertyChanged) this).PropertyChanged += (o, e) =>
        {
            if (e.PropertyName == nameof(Text))
            {
                _textLines = Text?.Split(Environment.NewLine);
            }
        };
    }

    protected override Size CalculateSize() => new(_textLines?.Max(l => l.Length) ?? 0, _textLines?.Length ?? 0);

    protected override bool DefaultRenderer(in RenderContext renderContext, Position position, Size size)
    {
        if (size.Width == 0 || size.Height == 0) return false;

        var foreground = Foreground ?? renderContext.Foreground;
        var background = Background ?? renderContext.Background;
        var renderState = new RenderState(
            position,
            size,
            Text,
            foreground,
            background,
            _textFormat);

        if (!renderContext.ForceRerender && !NeedsRerender(renderState)) return false;

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

        var driver = renderContext.ConsoleDriver;
        SetStyleColor(renderContext, foreground, background, _textFormat);

        RenderText(_textLines, driver, position, size, TransformText);

        return true;
    }

    private string TransformText(string text, Position position, Size size)
        => TextAlignment switch
        {
            TextAlignment.Right => string.Format($"{{0,{size.Width}}}", text),
            _ => string.Format($"{{0,{-size.Width}}}", text)
        };

    private bool NeedsRerender(RenderState renderState)
        => _lastRenderState is null || _lastRenderState != renderState;
}