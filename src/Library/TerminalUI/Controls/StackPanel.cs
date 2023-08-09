using System.Collections.ObjectModel;
using PropertyChanged.SourceGenerator;
using TerminalUI.Models;

namespace TerminalUI.Controls;

public partial class StackPanel<T> : ChildContainerView<T>
{
    private readonly Dictionary<IView, Size> _requestedSizes = new();
    [Notify] private Orientation _orientation = Orientation.Vertical;

    public override Size GetRequestedSize()
    {
        _requestedSizes.Clear();
        var width = 0;
        var height = 0;

        foreach (var child in Children)
        {
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

    protected override void DefaultRenderer(Position position, Size size)
    {
        var delta = 0;
        foreach (var child in Children)
        {
            if (!_requestedSizes.TryGetValue(child, out var childSize)) throw new Exception("Child size not found");
            var childPosition = Orientation == Orientation.Vertical
                ? position with {Y = position.Y + delta}
                : position with {X = position.X + delta};
            child.Render(childPosition, childSize);

            delta += Orientation == Orientation.Vertical
                ? childSize.Height
                : childSize.Width;
        }
    }
}