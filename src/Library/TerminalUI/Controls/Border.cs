using PropertyChanged.SourceGenerator;
using TerminalUI.Models;

namespace TerminalUI.Controls;

public partial class Border<T> : ContentView<T>
{
    [Notify] private Thickness _borderThickness = 1;
    [Notify] private Thickness _padding = 0;
    [Notify] private char _topChar = '─';
    [Notify] private char _leftChar = '│';
    [Notify] private char _rightChar = '│';
    [Notify] private char _bottomChar = '─';
    [Notify] private char _topLeftChar = '┌';
    [Notify] private char _topRightChar = '┐';
    [Notify] private char _bottomLeftChar = '└';
    [Notify] private char _bottomRightChar = '┘';

    public Border()
    {
        RerenderProperties.Add(nameof(BorderThickness));
        RerenderProperties.Add(nameof(Padding));
        RerenderProperties.Add(nameof(TopChar));
        RerenderProperties.Add(nameof(LeftChar));
        RerenderProperties.Add(nameof(RightChar));
        RerenderProperties.Add(nameof(BottomChar));
    }

    protected override Size CalculateSize()
    {
        var size = new Size(
            _borderThickness.Left + _borderThickness.Right + _padding.Left + _padding.Right,
            _borderThickness.Top + _borderThickness.Bottom + _padding.Top + _padding.Bottom
        );
        if (Content is null || !Content.IsVisible) return size;

        var contentSize = Content.GetRequestedSize();
        return new Size(contentSize.Width + size.Width, contentSize.Height + size.Height);
    }

    protected override bool DefaultRenderer(in RenderContext renderContext, Position position, Size size)
    {
        if (ContentRendererMethod is null)
        {
            throw new NullReferenceException(
                nameof(ContentRendererMethod)
                + " is null, cannot render content of "
                + Content?.GetType().Name
                + " with DataContext of "
                + DataContext?.GetType().Name);
        }

        var childPosition = new Position(X: position.X + _borderThickness.Left, Y: position.Y + _borderThickness.Top);
        var childSize = new Size(
            Width: size.Width - _borderThickness.Left - _borderThickness.Right,
            Height: size.Height - _borderThickness.Top - _borderThickness.Bottom
        );

        if (_padding.Left > 0 || _padding.Top > 0 || _padding.Right > 0 || _padding.Bottom > 0)
        {
            childPosition = new Position(X: childPosition.X + _padding.Left, Y: childPosition.Y + _padding.Top);
            childSize = new Size(
                Width: childSize.Width - _padding.Left - _padding.Right,
                Height: childSize.Height - _padding.Top - _padding.Bottom
            );
        }

        var contentRendered = ContentRendererMethod(renderContext, childPosition, childSize);

        if (contentRendered)
        {
            var driver = renderContext.ConsoleDriver;
            driver.ResetColor();
            SetColorsForDriver(renderContext);

            RenderTopBorder(renderContext, position, size);
            RenderBottomBorder(renderContext, position, size);
            RenderLeftBorder(renderContext, position, size);
            RenderRightBorder(renderContext, position, size);

            RenderTopLeftCorner(renderContext, position);
            RenderTopRightCorner(renderContext, position, size);
            RenderBottomLeftCorner(renderContext, position, size);
            RenderBottomRightCorner(renderContext, position, size);

            //TODO render padding
        }

        return contentRendered;
    }

    private void RenderTopBorder(in RenderContext renderContext, Position position, Size size)
    {
        position = position with {X = position.X + _borderThickness.Left};
        size = new Size(Width: size.Width - _borderThickness.Left - _borderThickness.Right, Height: _borderThickness.Top);
        RenderText(_topChar, renderContext.ConsoleDriver, position, size);
    }

    private void RenderBottomBorder(in RenderContext renderContext, Position position, Size size)
    {
        position = new Position(X: position.X + _borderThickness.Left, Y: position.Y + size.Height - _borderThickness.Bottom);
        size = new Size(Width: size.Width - _borderThickness.Left - _borderThickness.Right, Height: _borderThickness.Bottom);
        RenderText(_bottomChar, renderContext.ConsoleDriver, position, size);
    }

    private void RenderLeftBorder(in RenderContext renderContext, Position position, Size size)
    {
        position = position with {Y = position.Y + _borderThickness.Top};
        size = new Size(Width: _borderThickness.Left, Height: size.Height - _borderThickness.Top - _borderThickness.Bottom);
        RenderText(_leftChar, renderContext.ConsoleDriver, position, size);
    }

    private void RenderRightBorder(in RenderContext renderContext, Position position, Size size)
    {
        position = new Position(X: position.X + size.Width - _borderThickness.Right, Y: position.Y + _borderThickness.Top);
        size = new Size(Width: _borderThickness.Right, Height: size.Height - _borderThickness.Top - _borderThickness.Bottom);
        RenderText(_rightChar, renderContext.ConsoleDriver, position, size);
    }

    private void RenderTopLeftCorner(in RenderContext renderContext, Position position)
    {
        if (_borderThickness.Left == 0 || _borderThickness.Top == 0) return;

        var size = new Size(Width: _borderThickness.Left, Height: _borderThickness.Top);
        RenderText(_topLeftChar, renderContext.ConsoleDriver, position, size);
    }

    private void RenderTopRightCorner(in RenderContext renderContext, Position position, Size size)
    {
        if (_borderThickness.Right == 0 || _borderThickness.Top == 0) return;

        position = position with {X = position.X + size.Width - _borderThickness.Right};
        size = new Size(Width: _borderThickness.Right, Height: _borderThickness.Top);
        RenderText(_topRightChar, renderContext.ConsoleDriver, position, size);
    }

    private void RenderBottomLeftCorner(in RenderContext renderContext, Position position, Size size)
    {
        if (_borderThickness.Left == 0 || _borderThickness.Bottom == 0) return;

        position = position with {Y = position.Y + size.Height - _borderThickness.Bottom};
        size = new Size(Width: _borderThickness.Left, Height: _borderThickness.Bottom);
        RenderText(_bottomLeftChar, renderContext.ConsoleDriver, position, size);
    }

    private void RenderBottomRightCorner(in RenderContext renderContext, Position position, Size size)
    {
        if (_borderThickness.Right == 0 || _borderThickness.Bottom == 0) return;

        position = new Position(
            X: position.X + size.Width - _borderThickness.Right,
            Y: position.Y + size.Height - _borderThickness.Bottom
        );
        size = new Size(Width: _borderThickness.Right, Height: _borderThickness.Bottom);
        RenderText(_bottomRightChar, renderContext.ConsoleDriver, position, size);
    }
}