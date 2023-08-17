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
        IColor? Color);

    private RenderState? _lastRenderState;

    [Notify] private IColor? _fill;

    public Rectangle()
    {
        RerenderProperties.Add(nameof(Fill));
    }

    protected override Size CalculateSize() => new(Width ?? 0, Height ?? 0);

    protected override bool DefaultRenderer(in RenderContext renderContext, Position position, Size size)
    {
        var fillColor = Fill ?? Background ?? renderContext.Background;
        var renderState = new RenderState(position, size, fillColor);
        var skipRender = !renderContext.ForceRerender && !NeedsRerender(renderState);
        _lastRenderState = renderState;

        var driver = renderContext.ConsoleDriver;

        driver.ResetStyle();
        if (fillColor is not null)
        {
            driver.SetBackgroundColor(fillColor);
        }

        RenderEmpty(
            renderContext,
            position,
            size,
            skipRender,
            false
        );

        return !skipRender;
    }

    private bool NeedsRerender(RenderState renderState)
        => _lastRenderState is null || _lastRenderState != renderState;
}