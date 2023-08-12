﻿using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TerminalUI.Extensions;
using TerminalUI.Models;
using TerminalUI.Traits;
using TerminalUI.ViewExtensions;

namespace TerminalUI.Controls;

public class Grid<T> : ChildContainerView<T>, IVisibilityChangeHandler
{
    private readonly List<IView> _forceRerenderChildren = new();
    private readonly object _forceRerenderChildrenLock = new();
    private List<RowDefinition> _rowDefinitions = new() {RowDefinition.Star(1)};
    private List<ColumnDefinition> _columnDefinitions = new() {ColumnDefinition.Star(1)};
    private ILogger<Grid<T>>? Logger => ApplicationContext?.LoggerFactory?.CreateLogger<Grid<T>>();

    private delegate void WithSizes(RenderContext renderContext, ReadOnlySpan<int> widths, ReadOnlySpan<int> heights);

    private delegate TResult WithSizes<TResult>(RenderContext renderContext, ReadOnlySpan<int> widths, ReadOnlySpan<int> heights);

    private const int ToBeCalculated = -1;

    public IReadOnlyList<RowDefinition> RowDefinitions
    {
        get => _rowDefinitions;
        set
        {
            var nextValue = value;
            if (value.Count == 0)
            {
                nextValue = new List<RowDefinition> {RowDefinition.Star(1)};
            }

            var needUpdate = nextValue.Count != _rowDefinitions.Count;
            if (!needUpdate)
            {
                for (var i = 0; i < nextValue.Count; i++)
                {
                    if (!nextValue[i].Equals(_rowDefinitions[i]))
                    {
                        needUpdate = true;
                        break;
                    }
                }
            }

            if (needUpdate)
            {
                _rowDefinitions = nextValue.ToList();
                OnPropertyChanged();
            }
        }
    }

    public IReadOnlyList<ColumnDefinition> ColumnDefinitions
    {
        get => _columnDefinitions;
        set
        {
            var nextValue = value;
            if (value.Count == 0)
            {
                nextValue = new List<ColumnDefinition> {ColumnDefinition.Star(1)};
            }

            var needUpdate = nextValue.Count != _columnDefinitions.Count;
            if (!needUpdate)
            {
                for (var i = 0; i < nextValue.Count; i++)
                {
                    if (!nextValue[i].Equals(_columnDefinitions[i]))
                    {
                        needUpdate = true;
                        break;
                    }
                }
            }

            if (needUpdate)
            {
                _columnDefinitions = nextValue.ToList();
                OnPropertyChanged();
            }
        }
    }

    public object? ColumnDefinitionsObject
    {
        get => ColumnDefinitions;
        set
        {
            if (value is IEnumerable<ColumnDefinition> columnDefinitions)
            {
                ColumnDefinitions = columnDefinitions.ToList();
            }
            else if (value is string s)
            {
                SetColumnDefinitions(s);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    public object? RowDefinitionsObject
    {
        get => RowDefinitions;
        set
        {
            if (value is IEnumerable<RowDefinition> rowDefinitions)
            {
                RowDefinitions = rowDefinitions.ToList();
            }
            else if (value is string s)
            {
                SetRowDefinitions(s);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    protected override Size CalculateSize()
        => WithCalculatedSize(
            RenderContext.Empty,
            new Option<Size>(new Size(0, 0), false),
            (_, columnWidths, rowHeights) =>
            {
                var width = 0;
                var height = 0;

                foreach (var t in columnWidths)
                {
                    width += t;
                }

                foreach (var t in rowHeights)
                {
                    height += t;
                }

                return new Size(width, height);
            });

    protected override bool DefaultRenderer(RenderContext renderContext, Position position, Size size)
        => WithCalculatedSize(
            renderContext,
            new Option<Size>(size, true),
            (context, columnWidths, rowHeights) =>
            {
                IReadOnlyList<IView> forceRerenderChildren;
                lock (_forceRerenderChildrenLock)
                {
                    forceRerenderChildren = _forceRerenderChildren.ToList();
                    _forceRerenderChildren.Clear();
                }

                context = new RenderContext(
                    context.ConsoleDriver,
                    context.ForceRerender,
                    Foreground ?? context.Foreground,
                    Background ?? context.Background
                );
                var viewsByPosition = GroupViewsByPosition(columnWidths.Length, rowHeights.Length);

                for (var column = 0; column < columnWidths.Length; column++)
                {
                    for (var row = 0; row < rowHeights.Length; row++)
                    {
                        RenderViewsByPosition(
                            context,
                            position,
                            columnWidths,
                            rowHeights,
                            viewsByPosition,
                            column,
                            row,
                            forceRerenderChildren
                        );
                    }
                }

                return true;
            });

    private void RenderViewsByPosition(RenderContext context,
        Position gridPosition,
        ReadOnlySpan<int> columnWidths,
        ReadOnlySpan<int> rowHeights,
        IReadOnlyDictionary<(int, int), List<IView>> viewsByPosition,
        int column,
        int row,
        IReadOnlyList<IView> forceRerenderChildren)
    {
        if (!viewsByPosition.TryGetValue((column, row), out var children)) return;

        var width = columnWidths[column];
        var height = rowHeights[row];
        var renderSize = new Size(width, height);

        var renderPosition = GetRenderPosition(
            gridPosition,
            columnWidths,
            rowHeights,
            column,
            row
        );

        var needsRerender = children.Any(forceRerenderChildren.Contains);
        if (needsRerender)
        {
            context = new RenderContext(
                context.ConsoleDriver,
                true,
                context.Foreground,
                context.Background
            );
            RenderEmpty(context, renderPosition, renderSize);
        }

        //This implies that children further back in the list will be rendered on top of children placed before in the list.
        foreach (var child in children.Where(child => child.IsVisible))
        {
            var rendered = child.Render(context, renderPosition, renderSize);
            if (rendered && !needsRerender)
            {
                needsRerender = true;
                context = new RenderContext(
                    context.ConsoleDriver,
                    true,
                    context.Foreground,
                    context.Background
                );
            }
        }

        static Position GetRenderPosition(
            Position gridPosition,
            ReadOnlySpan<int> columnWidths,
            ReadOnlySpan<int> rowHeights,
            int column,
            int row
        )
        {
            var left = gridPosition.X;
            var top = gridPosition.Y;

            for (var i = 0; i < column; i++)
            {
                left += columnWidths[i];
            }

            for (var i = 0; i < row; i++)
            {
                top += rowHeights[i];
            }

            return new Position(left, top);
        }
    }

    private Dictionary<(int, int), List<IView>> GroupViewsByPosition(int columns, int rows)
    {
        Dictionary<ValueTuple<int, int>, List<IView>> viewsByPosition = new();
        foreach (var child in Children)
        {
            var (x, y) = GetViewColumnAndRow(child, columns, rows);
            if (viewsByPosition.TryGetValue((x, y), out var list))
            {
                list.Add(child);
            }
            else
            {
                viewsByPosition[(x, y)] = new List<IView> {child};
            }
        }

        return viewsByPosition;
    }

    private ValueTuple<int, int> GetViewColumnAndRow(IView view, int columns, int rows)
    {
        var positionExtension = view.GetExtension<GridPositionExtension>();
        var x = positionExtension?.Column ?? 0;
        var y = positionExtension?.Row ?? 0;

        if (x > columns)
        {
            Logger?.LogWarning("Child {Child} is out of bounds, x: {X}, y: {Y}", view, x, y);
            x = 0;
        }

        if (y > rows)
        {
            Logger?.LogWarning("Child {Child} is out of bounds, x: {X}, y: {Y}", view, x, y);
            y = 0;
        }

        return (x, y);
    }

    private void WithCalculatedSize(RenderContext renderContext, Option<Size> size, WithSizes actionWithSizes)
    {
        WithCalculatedSize(renderContext, size, Helper);

        object? Helper(RenderContext renderContext1, ReadOnlySpan<int> widths, ReadOnlySpan<int> heights)
        {
            actionWithSizes(renderContext1, widths, heights);
            return null;
        }
    }

    private TResult WithCalculatedSize<TResult>(RenderContext renderContext, Option<Size> size, WithSizes<TResult> actionWithSizes)
    {
        //TODO: Optimize it, dont calculate all of these, only if there is Auto value(s)
        var columns = ColumnDefinitions.Count;
        var rows = RowDefinitions.Count;

        Debug.Assert(columns > 0, "Columns must contain at least one element");
        Debug.Assert(rows > 0, "Rows must contain at least one element");

        Span<int> allWidth = stackalloc int[columns * rows];
        Span<int> allHeight = stackalloc int[columns * rows];

        foreach (var child in Children)
        {
            var childSize = child.GetRequestedSize();
            var (x, y) = GetViewColumnAndRow(child, columns, rows);

            allWidth.SetToMatrix(childSize.Width, x, y, columns);
            allHeight.SetToMatrix(childSize.Height, x, y, columns);
        }

        Span<int> columnWidths = stackalloc int[columns];
        Span<int> rowHeights = stackalloc int[rows];

        var usedWidth = 0;
        var widthStars = 0;
        for (var i = 0; i < columnWidths.Length; i++)
        {
            if (ColumnDefinitions[i].Type == GridUnitType.Pixel)
            {
                columnWidths[i] = ColumnDefinitions[i].Value;
            }
            else if (size.IsSome && ColumnDefinitions[i].Type == GridUnitType.Star)
            {
                widthStars += ColumnDefinitions[i].Value;
                columnWidths[i] = ToBeCalculated;
            }
            else
            {
                var max = 0;
                for (var j = 0; j < rows; j++)
                {
                    max = Math.Max(max, allWidth.GetFromMatrix(i, j, columns));
                }

                columnWidths[i] = max;
            }

            if (columnWidths[i] != ToBeCalculated)
                usedWidth += columnWidths[i];
        }

        var usedHeight = 0;
        var heightStars = 0;
        for (var i = 0; i < rowHeights.Length; i++)
        {
            if (RowDefinitions[i].Type == GridUnitType.Pixel)
            {
                rowHeights[i] = RowDefinitions[i].Value;
            }
            else if (size.IsSome && RowDefinitions[i].Type == GridUnitType.Star)
            {
                heightStars += RowDefinitions[i].Value;
                rowHeights[i] = ToBeCalculated;
            }
            else
            {
                var max = 0;
                for (var j = 0; j < columns; j++)
                {
                    max = Math.Max(max, allHeight.GetFromMatrix(j, i, columns));
                }

                rowHeights[i] = max;
            }

            if (rowHeights[i] != ToBeCalculated)
                usedHeight += rowHeights[i];
        }

        if (size.IsSome)
        {
            var widthLeft = size.Value.Width - usedWidth;
            var heightLeft = size.Value.Height - usedHeight;

            var widthPerStart = (int) Math.Floor((double) widthLeft / widthStars);
            var heightPerStart = (int) Math.Floor((double) heightLeft / heightStars);

            for (var i = 0; i < columnWidths.Length; i++)
            {
                var column = ColumnDefinitions[i];
                if (column.Type == GridUnitType.Star)
                {
                    columnWidths[i] = widthPerStart * column.Value;
                }
            }

            for (var i = 0; i < rowHeights.Length; i++)
            {
                var row = RowDefinitions[i];
                if (row.Type == GridUnitType.Star)
                {
                    rowHeights[i] = heightPerStart * row.Value;
                }
            }
        }

        return actionWithSizes(renderContext, columnWidths, rowHeights);
    }

    public void SetRowDefinitions(string value)
    {
        var values = value.Split(' ');
        var rowDefinitions = new List<RowDefinition>();

        foreach (var v in values)
        {
            if (v == "Auto")
            {
                rowDefinitions.Add(RowDefinition.Auto);
            }
            else if (v.EndsWith("*"))
            {
                var starValue = v.Length == 1 ? 1 : int.Parse(v[..^1]);
                rowDefinitions.Add(RowDefinition.Star(starValue));
            }
            else if (int.TryParse(v, out var pixelValue))
            {
                rowDefinitions.Add(RowDefinition.Pixel(pixelValue));
            }
            else
            {
                throw new ArgumentException("Invalid row definition: " + v);
            }
        }

        RowDefinitions = rowDefinitions;
    }

    public void SetColumnDefinitions(string value)
    {
        var values = value.Split(' ');
        var columnDefinitions = new List<ColumnDefinition>();

        foreach (var v in values)
        {
            if (v == "Auto")
            {
                columnDefinitions.Add(ColumnDefinition.Auto);
            }
            else if (v.EndsWith("*"))
            {
                var starValue = v.Length == 1 ? 1 : int.Parse(v[..^1]);
                columnDefinitions.Add(ColumnDefinition.Star(starValue));
            }
            else if (int.TryParse(v, out var pixelValue))
            {
                columnDefinitions.Add(ColumnDefinition.Pixel(pixelValue));
            }
            else
            {
                throw new ArgumentException("Invalid column definition: " + v);
            }
        }

        ColumnDefinitions = columnDefinitions;
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