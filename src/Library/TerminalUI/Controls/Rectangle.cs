using PropertyChanged.SourceGenerator;
using TerminalUI.Color;
using TerminalUI.Models;
using TerminalUI.Traits;

namespace TerminalUI.Controls;

public sealed partial class Rectangle<T> : View<Rectangle<T>, T>, IDisplayView
{
    private record RenderState(
        Position Position,
        Size Size,
        IColor? Fill);

    private RenderState? _lastRenderState;

    [Notify] private IColor? _fill;
    protected override Size CalculateSize() => new(Width ?? 0, Height ?? 0);

    protected override bool DefaultRenderer(in RenderContext renderContext, Position position, Size size)
    {
        var renderState = new RenderState(position, size, Fill);
        if ((!renderContext.ForceRerender && !NeedsRerender(renderState)) || Fill is null) return false;
        _lastRenderState = renderState;

        var driver = renderContext.ConsoleDriver;

        var s = new string('█', size.Width);
        driver.SetBackgroundColor(Fill);
        driver.SetForegroundColor(Fill);

        var height = size.Height;
        for (var i = 0; i < height; i++)
        {
            driver.SetCursorPosition(position with {Y = position.Y + i});
            driver.Write(s);
        }

        return true;
    }

    private bool NeedsRerender(RenderState renderState)
        => _lastRenderState is null || _lastRenderState != renderState;
}