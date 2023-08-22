using System.Reflection.Metadata;
using PropertyChanged.SourceGenerator;
using TerminalUI.Color;
using TerminalUI.Models;
using TerminalUI.Traits;

namespace TerminalUI.Controls;

public partial class BinaryView<T> : View<BinaryView<T>, T>, IDisplayView
{
    private record RenderState(
        byte[]? Data,
        int BytesPerLine,
        Position Position,
        Size Size,
        IColor? Foreground,
        IColor? Background);

    private RenderState? _lastRenderState;

    [Notify] private byte[]? _data;
    [Notify] private int _bytesPerLine = 16;

    public BinaryView()
    {
        RerenderProperties.Add(nameof(Data));
        RerenderProperties.Add(nameof(BytesPerLine));
    }

    protected override Size CalculateSize()
    {
        if (_data is null) return new(0, 0);
        if (_data.Length < _bytesPerLine) return new Size(_data.Length, 1);

        var completeLines = (int) Math.Floor((double) _data.Length / _bytesPerLine);
        var remaining = completeLines * _bytesPerLine - completeLines;

        var lines = remaining > 0 ? completeLines + 1 : completeLines;

        return new Size(_bytesPerLine * 3 - 1, lines);
    }

    protected override bool DefaultRenderer(in RenderContext renderContext, Position position, Size size)
    {
        if (size.Width < 2 || size.Height == 0) return false;

        var data = _data;
        var bytesPerLine = _bytesPerLine;
        if (size.Width < _bytesPerLine * 3 - 1)
        {
            bytesPerLine = (int) Math.Floor((double) (size.Width + 1) / 3);
        }

        var foreground = Foreground ?? renderContext.Foreground;
        var background = Background ?? renderContext.Background;

        var renderState = new RenderState(
            data,
            bytesPerLine,
            position,
            size,
            foreground,
            background);

        var skipRender = !renderContext.ForceRerender && !NeedsRerender(renderState);
        _lastRenderState = renderState;

        if (data is null) return false;
        
        var driver = renderContext.ConsoleDriver;
        driver.ResetStyle();
        SetStyleColor(renderContext, foreground, background);

        var lineI = 0;
        var textSize = size with {Height = 1};
        for (var i = 0; i < data.Length; i += bytesPerLine, lineI++)
        {
            if (lineI > size.Height) break;
            RenderLine(
                renderContext,
                data,
                i,
                i + bytesPerLine,
                position with {Y = position.Y + lineI},
                textSize,
                skipRender);
        }

        return true;
    }

    private void RenderLine(
        in RenderContext renderContext,
        byte[] data,
        int startIndex,
        int maxEndIndex,
        Position position,
        Size size,
        bool updateCellsOnly
    )
    {
        Span<char> text = stackalloc char[(maxEndIndex - startIndex) * 3 - 1];
        var textI = 0;
        for (var i = startIndex; i < maxEndIndex && i < data.Length; i++, textI += 3)
        {
            var b = data[i];
            var b1 = (byte) (b >> 4);
            var b2 = (byte) (b & 0x0F);

            var c1 = b1 < 10 ? (char) (b1 + '0') : (char) (b1 - 10 + 'A');
            var c2 = b2 < 10 ? (char) (b2 + '0') : (char) (b2 - 10 + 'A');
            text[textI] = c1;
            text[textI + 1] = c2;

            if (textI + 2 < text.Length)
                text[textI + 2] = ' ';
        }

        RenderText(
            text,
            renderContext,
            position,
            size,
            updateCellsOnly
        );
    }

    private bool NeedsRerender(RenderState renderState)
        => _lastRenderState is null || _lastRenderState != renderState;
}