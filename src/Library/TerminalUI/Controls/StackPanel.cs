using PropertyChanged.SourceGenerator;
using TerminalUI.Models;
using TerminalUI.Traits;

namespace TerminalUI.Controls;

public sealed partial class StackPanel<T> : ChildContainerView<StackPanel<T>, T>, IVisibilityChangeHandler
{
    private readonly List<IView> _forceRerenderChildren = new();
    private readonly object _forceRerenderChildrenLock = new();
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

    protected override bool DefaultRenderer(in RenderContext renderContext, Position position, Size size)
    {
        var neededRerender = false;
        IReadOnlyList<IView> forceRerenderChildren;
        lock (_forceRerenderChildrenLock)
        {
            forceRerenderChildren = _forceRerenderChildren.ToList();
            _forceRerenderChildren.Clear();
        }

        var delta = 0;
        foreach (var child in Children)
        {
            if (!child.IsVisible) continue;

            if (!_requestedSizes.TryGetValue(child, out var childSize)) throw new Exception("Child size not found");

            var childPosition = Orientation == Orientation.Vertical
                ? position with {Y = position.Y + delta}
                : position with {X = position.X + delta};

            childSize = Orientation == Orientation.Vertical
                ? childSize with {Width = size.Width}
                : childSize with {Height = size.Height};

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

            if (forceRerenderChildren.Contains(child))
            {
                var rerenderContext = renderContext with {ForceRerender = true};
                neededRerender = child.Render(rerenderContext, childPosition, childSize) || neededRerender;
            }
            else
            {
                neededRerender = child.Render(renderContext, childPosition, childSize) || neededRerender;
            }

            delta += Orientation == Orientation.Vertical
                ? childSize.Height
                : childSize.Width;
        }

        if (Orientation == Orientation.Horizontal)
        {
            var leftWidth = size.Width - delta;
            Span<char> text = stackalloc char[leftWidth];
            text.Fill(ApplicationContext!.EmptyCharacter);

            SetColorsForDriver(renderContext);
            RenderText(
                text,
                renderContext.ConsoleDriver,
                position with {X = position.X + delta},
                size with {Width = leftWidth}
            );
        }
        else
        {
            var leftHeight = size.Height - delta;
            Span<char> text = stackalloc char[size.Width];
            text.Fill(ApplicationContext!.EmptyCharacter);

            SetColorsForDriver(renderContext);
            RenderText(
                text,
                renderContext.ConsoleDriver,
                position with {Y = position.Y + delta},
                size with {Height = leftHeight}
            );
        }

        return neededRerender;
    }

    public void ChildVisibilityChanged(IView child)
    {
        var viewToForceRerender = child;
        while (viewToForceRerender.VisualParent != null && viewToForceRerender.VisualParent != this)
        {
            viewToForceRerender = viewToForceRerender.VisualParent;
        }

        if (viewToForceRerender.VisualParent != this) return;

        lock (_forceRerenderChildrenLock)
        {
            _forceRerenderChildren.Add(viewToForceRerender);
        }
    }
}