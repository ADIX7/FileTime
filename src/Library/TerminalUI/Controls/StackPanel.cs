using System.Collections.ObjectModel;
using PropertyChanged.SourceGenerator;
using TerminalUI.Models;

namespace TerminalUI.Controls;

public partial class StackPanel<T> : ChildContainerView<T>
{
    private readonly Dictionary<IView, Size> _requestedSizes = new();
    [Notify] private Orientation _orientation = Orientation.Vertical;

    protected override Size CalculateSize()
    {
        _requestedSizes.Clear();
        var width = 0;
        var height = 0;

        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;

            var childSize = child.GetRequestedSize();
            _requestedSizes.Add(child, childSize);

            if (Orientation == Orientation.Vertical)
            {
                width = Math.Max(width, childSize.Width);
                height += childSize.Height;
            }
            else
            {
                width += childSize.Width;
                height = Math.Max(height, childSize.Height);
            }
        }

        return new Size(width, height);
    }

    protected override bool DefaultRenderer(RenderContext renderContext, Position position, Size size)
    {
        var delta = 0;
        var neededRerender = false;
        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;

            if (!_requestedSizes.TryGetValue(child, out var childSize)) throw new Exception("Child size not found");

            var childPosition = Orientation == Orientation.Vertical
                ? position with {Y = position.Y + delta}
                : position with {X = position.X + delta};

            var endX = position.X + size.Width;
            var endY = position.Y + size.Height;

            if (childPosition.X > endX || childPosition.Y > endY) break;
            if (childPosition.X + childSize.Width > endX)
            {
                childSize = childSize with {Width = endX - childPosition.X};
            }

            if (childPosition.Y + childSize.Height > endY)
            {
                childSize = childSize with {Height = endY - childPosition.Y};
            }

            neededRerender = child.Render(renderContext, childPosition, childSize) || neededRerender;

            delta += Orientation == Orientation.Vertical
                ? childSize.Height
                : childSize.Width;
        }

        return neededRerender;
    }
}