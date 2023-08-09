using System.Collections.ObjectModel;
using TerminalUI.Extensions;
using TerminalUI.Models;
using TerminalUI.ViewExtensions;

namespace TerminalUI.Controls;

public class Grid<T> : View<T>
{
    private const int ToBeCalculated = -1;
    private readonly ObservableCollection<IView> _children = new();
    public ReadOnlyObservableCollection<IView> Children { get; }
    public GridChildInitializer<T> ChildInitializer { get; }
    public ObservableCollection<RowDefinition> RowDefinitions { get; } = new();
    public ObservableCollection<ColumnDefinition> ColumnDefinitions { get; } = new();

    public object? ColumnDefinitionsObject
    {
        get => ColumnDefinitions;
        set
        {
            if (value is IEnumerable<ColumnDefinition> columnDefinitions)
            {
                ColumnDefinitions.Clear();
                foreach (var columnDefinition in columnDefinitions)
                {
                    ColumnDefinitions.Add(columnDefinition);
                }
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
                RowDefinitions.Clear();
                foreach (var rowDefinition in rowDefinitions)
                {
                    RowDefinitions.Add(rowDefinition);
                }
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

    public Grid()
    {
        ChildInitializer = new GridChildInitializer<T>(this);
        Children = new ReadOnlyObservableCollection<IView>(_children);
        _children.CollectionChanged += (o, e) =>
        {
            if (Attached)
            {
                if (e.NewItems?.OfType<IView>() is { } newItems)
                {
                    foreach (var newItem in newItems)
                    {
                        newItem.Attached = true;
                    }
                }

                ApplicationContext?.EventLoop.RequestRerender();
            }
        };
    }

    public override Size GetRequestedSize() => throw new NotImplementedException();

    protected override void DefaultRenderer(Position position, Size size)
    {
        //TODO: Optimize it, dont calculate all of these only if there is Auto value(s)
        var columns = ColumnDefinitions.Count;
        Span<int> allWidth = stackalloc int[columns * RowDefinitions.Count];
        Span<int> allHeight = stackalloc int[columns * RowDefinitions.Count];

        foreach (var child in Children)
        {
            var childSize = child.GetRequestedSize();
            var positionExtension = child.GetExtension<GridPositionExtension>();
            var x = positionExtension?.Column ?? 0;
            var y = positionExtension?.Row ?? 0;

            allWidth.SetToMatrix(childSize.Width, x, y, columns);
            allHeight.SetToMatrix(childSize.Height, x, y, columns);
        }

        Span<int> columnWidths = stackalloc int[columns];
        Span<int> rowHeights = stackalloc int[RowDefinitions.Count];

        for (var i = 0; i < columnWidths.Length; i++)
        {
            if (ColumnDefinitions[i].Type == GridUnitType.Pixel)
            {
                columnWidths[i] = ColumnDefinitions[i].Value;
            }
            else if (ColumnDefinitions[i].Type == GridUnitType.Star)
            {
                columnWidths[i] = ToBeCalculated;
            }
            else
            {
                var max = 0;
                for (var j = 0; j < RowDefinitions.Count; j++)
                {
                    max = Math.Max(max, allWidth.GetFromMatrix(i, j, columns));
                }

                columnWidths[i] = max;
            }
        }

        for (var i = 0; i < rowHeights.Length; i++)
        {
            if (RowDefinitions[i].Type == GridUnitType.Pixel)
            {
                rowHeights[i] = RowDefinitions[i].Value;
            }
            else if (RowDefinitions[i].Type == GridUnitType.Star)
            {
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
        }

        foreach (var child in Children)
        {
            var childSize = child.GetRequestedSize();
            var positionExtension = child.GetExtension<GridPositionExtension>();
            var x = positionExtension?.Column ?? 0;
            var y = positionExtension?.Row ?? 0;

            var width = columnWidths[x];
            var height = rowHeights[y];

            var left = 0;
            var top = 0;

            for (var i = 0; i < x; i++)
            {
                left += columnWidths[i];
            }

            for (var i = 0; i < y; i++)
            {
                top += rowHeights[i];
            }

            child.Render(new Position(left, top), new Size(width, height));
        }
    }

    public void SetRowDefinitions(string value)
    {
        var values = value.Split(' ');
        RowDefinitions.Clear();

        foreach (var v in values)
        {
            if (v == "Auto")
            {
                RowDefinitions.Add(RowDefinition.Auto);
            }
            else if (v.EndsWith("*"))
            {
                var starValue = int.Parse(v[0..^1]);
                RowDefinitions.Add(RowDefinition.Star(starValue));
            }
            else if (int.TryParse(v, out var pixelValue))
            {
                RowDefinitions.Add(RowDefinition.Pixel(pixelValue));
            }
            else
            {
                throw new ArgumentException("Invalid row definition: " + v);
            }
        }
    }

    public void SetColumnDefinitions(string value)
    {
        var values = value.Split(' ');
        ColumnDefinitions.Clear();

        foreach (var v in values)
        {
            if (v == "Auto")
            {
                ColumnDefinitions.Add(ColumnDefinition.Auto);
            }
            else if (v.EndsWith("*"))
            {
                var starValue = int.Parse(v[0..^1]);
                ColumnDefinitions.Add(ColumnDefinition.Star(starValue));
            }
            else if (int.TryParse(v, out var pixelValue))
            {
                ColumnDefinitions.Add(ColumnDefinition.Pixel(pixelValue));
            }
            else
            {
                throw new ArgumentException("Invalid column definition: " + v);
            }
        }
    }

    public override TChild AddChild<TChild>(TChild child)
    {
        child = base.AddChild(child);
        _children.Add(child);
        return child;
    }

    public override TChild AddChild<TChild, TDataContext>(TChild child, Func<T?, TDataContext?> dataContextMapper)
        where TDataContext : default
    {
        child = base.AddChild(child, dataContextMapper);
        _children.Add(child);
        return child;
    }
}