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
        ITextFormat? TextFormat,
        bool AsciiOnly);

    private RenderState? _lastRenderState;
    private string[]? _textLines;

    [Notify] private string? _text = string.Empty;
    [Notify] private TextAlignment _textAlignment = TextAlignment.Left;
    [Notify] private ITextFormat? _textFormat;
    [Notify] private int _textStartIndex;
    [Notify] private bool _asciiOnly = true;

    public TextBlock()
    {
        RerenderProperties.Add(nameof(Text));
        RerenderProperties.Add(nameof(TextAlignment));
        RerenderProperties.Add(nameof(TextFormat));
        RerenderProperties.Add(nameof(TextStartIndex));
        RerenderProperties.Add(nameof(AsciiOnly));

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
        var text = Text;
        var textLines = _textLines;
        var asciiOnly = _asciiOnly;
        
        var renderState = new RenderState(
            position,
            size,
            text,
            foreground,
            background,
            _textFormat,
            asciiOnly);

        var skipRender = !renderContext.ForceRerender && !NeedsRerender(renderState);

        _lastRenderState = renderState;

        var textStartIndex = _textStartIndex;
        if (textLines is null)
        {
            return false;
        }

        SetStyleColor(renderContext, foreground, background, _textFormat);

        if (textStartIndex < textLines.Length)
        {
            textLines = textLines[textStartIndex..];
        }
        else
        {
            _textStartIndex = textLines.Length - size.Height;
        }

        RenderText(textLines, renderContext, position, size, skipRender, TransformText, asciiOnly);

        return !skipRender;
    }

    private string TransformText(string text, Position position, Size size)
        => TextAlignment switch
        {
            TextAlignment.Right => string.Format($"{{0,{size.Width}}}", text),
            TextAlignment.Center => string.Format($"{{0,{(size.Width - text.Length) / 2 + text.Length}}}", text),
            _ => text
        };

    private bool NeedsRerender(RenderState renderState)
        => _lastRenderState is null || _lastRenderState != renderState;
}