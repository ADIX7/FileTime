using PropertyChanged.SourceGenerator;
using TerminalUI.Color;
using TerminalUI.Models;
using TerminalUI.Traits;

namespace TerminalUI.Controls;

public sealed partial class Border<T> : ContentView<Border<T>, T>, IDisplayView
{
    private record struct RenderState(
        Position Position,
        Size Size,
        Thickness BorderThickness,
        Thickness Padding,
        char TopChar,
        char LeftChar,
        char RightChar,
        char BottomChar,
        char TopLeftChar,
        char TopRightChar,
        char BottomLeftChar,
        char BottomRightChar,
        IColor? Fill
    );
    
    private RenderState _lastRenderState;
    
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
    [Notify] private IColor? _fill;

    public Border()
    {
        RerenderProperties.Add(nameof(BorderThickness));
        RerenderProperties.Add(nameof(Padding));
        RerenderProperties.Add(nameof(TopChar));
        RerenderProperties.Add(nameof(LeftChar));
        RerenderProperties.Add(nameof(RightChar));
        RerenderProperties.Add(nameof(BottomChar));
        RerenderProperties.Add(nameof(TopLeftChar));
        RerenderProperties.Add(nameof(TopRightChar));
        RerenderProperties.Add(nameof(Fill));
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

        var backgroundColor = Background ?? renderContext.Background;
        var foregroundColor = Foreground ?? renderContext.Foreground;
        var fillColor = Fill ?? Background ?? renderContext.Background;
        
        var renderState = new RenderState(
            position,
            size,
            _borderThickness,
            _padding,
            _topChar,
            _leftChar,
            _rightChar,
            _bottomChar,
            _topLeftChar,
            _topRightChar,
            _bottomLeftChar,
            _bottomRightChar,
            fillColor
        );
        var skipBorderRender = !renderContext.ForceRerender && !NeedsRerender(renderState);
        if (!skipBorderRender)
        {
            _lastRenderState = renderState;
        }

        var childPosition = new Position(X: position.X + _borderThickness.Left, Y: position.Y + _borderThickness.Top);
        var childSize = new Size(
            Width: size.Width - _borderThickness.Left - _borderThickness.Right,
            Height: size.Height - _borderThickness.Top - _borderThickness.Bottom
        );

        var childPositionWithoutPadding = childPosition;
        var childSizeWithoutPadding = childSize;

        if (_padding.Left > 0 || _padding.Top > 0 || _padding.Right > 0 || _padding.Bottom > 0)
        {
            childPosition = new Position(X: childPosition.X + _padding.Left, Y: childPosition.Y + _padding.Top);
            childSize = new Size(
                Width: childSize.Width - _padding.Left - _padding.Right,
                Height: childSize.Height - _padding.Top - _padding.Bottom
            );
        }

        // Same size as the original.
        // Although wasting memory, but we would have to delta the position when setting "updatedcells"
        // It is easier and also covers the fact the children use a different array
        var borderChildUpdatedCells = new bool[
            renderContext.UpdatedCells.GetLength(0),
            renderContext.UpdatedCells.GetLength(1)
        ];
        var childRenderContext = renderContext with {UpdatedCells = borderChildUpdatedCells};

        var contentRendered = ContentRendererMethod(childRenderContext, childPosition, childSize);

        if (contentRendered)
        {
            var driver = renderContext.ConsoleDriver;
            driver.ResetStyle();
            SetStyleColor(renderContext, foregroundColor, backgroundColor);
        }

        var updateCellsOnly = !contentRendered || skipBorderRender;
        RenderTopBorder(renderContext, position, size, updateCellsOnly);
        RenderBottomBorder(renderContext, position, size, updateCellsOnly);
        RenderLeftBorder(renderContext, position, size, updateCellsOnly);
        RenderRightBorder(renderContext, position, size, updateCellsOnly);

        RenderTopLeftCorner(renderContext, position, updateCellsOnly);
        RenderTopRightCorner(renderContext, position, size, updateCellsOnly);
        RenderBottomLeftCorner(renderContext, position, size, updateCellsOnly);
        RenderBottomRightCorner(renderContext, position, size, updateCellsOnly);

        if (fillColor != null)
        {
            SetStyleColor(renderContext, foregroundColor, fillColor);

            // Use the same array that children use. Also use that area, so we working only inside the border 
            Array2DHelper.RenderEmpty(
                renderContext.ConsoleDriver,
                borderChildUpdatedCells,
                borderChildUpdatedCells,
                ApplicationContext!.EmptyCharacter,
                childPositionWithoutPadding,
                childSizeWithoutPadding
            );
        }

        //Write back the changes to the original array
        Array2DHelper.CombineArray2Ds(
            renderContext.UpdatedCells,
            borderChildUpdatedCells,
            new Position(0, 0),
            renderContext.UpdatedCells,
            (a, b) => (a ?? false) || (b ?? false)
        );

        return contentRendered;
    }

    private bool NeedsRerender(RenderState renderState) => renderState != _lastRenderState;

    private void RenderTopBorder(in RenderContext renderContext, Position position, Size size, bool updateCellsOnly)
    {
        position = position with {X = position.X + _borderThickness.Left};
        size = new Size(Width: size.Width - _borderThickness.Left - _borderThickness.Right, Height: _borderThickness.Top);
        RenderText(_topChar, renderContext, position, size, updateCellsOnly);
    }

    private void RenderBottomBorder(in RenderContext renderContext, Position position, Size size, bool updateCellsOnly)
    {
        position = new Position(X: position.X + _borderThickness.Left, Y: position.Y + size.Height - _borderThickness.Bottom);
        size = new Size(Width: size.Width - _borderThickness.Left - _borderThickness.Right, Height: _borderThickness.Bottom);
        RenderText(_bottomChar, renderContext, position, size, updateCellsOnly);
    }

    private void RenderLeftBorder(in RenderContext renderContext, Position position, Size size, bool updateCellsOnly)
    {
        position = position with {Y = position.Y + _borderThickness.Top};
        size = new Size(Width: _borderThickness.Left, Height: size.Height - _borderThickness.Top - _borderThickness.Bottom);
        RenderText(_leftChar, renderContext, position, size, updateCellsOnly);
    }

    private void RenderRightBorder(in RenderContext renderContext, Position position, Size size, bool updateCellsOnly)
    {
        position = new Position(X: position.X + size.Width - _borderThickness.Right, Y: position.Y + _borderThickness.Top);
        size = new Size(Width: _borderThickness.Right, Height: size.Height - _borderThickness.Top - _borderThickness.Bottom);
        RenderText(_rightChar, renderContext, position, size, updateCellsOnly);
    }

    private void RenderTopLeftCorner(in RenderContext renderContext, Position position, bool updateCellsOnly)
    {
        if (_borderThickness.Left == 0 || _borderThickness.Top == 0) return;

        var size = new Size(Width: _borderThickness.Left, Height: _borderThickness.Top);
        RenderText(_topLeftChar, renderContext, position, size, updateCellsOnly);
    }

    private void RenderTopRightCorner(in RenderContext renderContext, Position position, Size size, bool updateCellsOnly)
    {
        if (_borderThickness.Right == 0 || _borderThickness.Top == 0) return;

        position = position with {X = position.X + size.Width - _borderThickness.Right};
        size = new Size(Width: _borderThickness.Right, Height: _borderThickness.Top);
        RenderText(_topRightChar, renderContext, position, size, updateCellsOnly);
    }

    private void RenderBottomLeftCorner(in RenderContext renderContext, Position position, Size size, bool updateCellsOnly)
    {
        if (_borderThickness.Left == 0 || _borderThickness.Bottom == 0) return;

        position = position with {Y = position.Y + size.Height - _borderThickness.Bottom};
        size = new Size(Width: _borderThickness.Left, Height: _borderThickness.Bottom);
        RenderText(_bottomLeftChar, renderContext, position, size, updateCellsOnly);
    }

    private void RenderBottomRightCorner(in RenderContext renderContext, Position position, Size size, bool updateCellsOnly)
    {
        if (_borderThickness.Right == 0 || _borderThickness.Bottom == 0) return;

        position = new Position(
            X: position.X + size.Width - _borderThickness.Right,
            Y: position.Y + size.Height - _borderThickness.Bottom
        );
        size = new Size(Width: _borderThickness.Right, Height: _borderThickness.Bottom);
        RenderText(_bottomRightChar, renderContext, position, size, updateCellsOnly);
    }
}