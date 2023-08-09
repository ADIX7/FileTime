using System.Collections.ObjectModel;
using TerminalUI.Extensions;
using TerminalUI.Models;
using TerminalUI.ViewExtensions;

namespace TerminalUI.Controls;

public class Grid<T> : ChildContainerView<T>
{
    private delegate void WithSizes(Span<int> widths, Span<int> heights);

    private delegate TResult WithSizes<TResult>(Span<int> widths, Span<int> heights);

    private const int ToBeCalculated = -1;
    public ObservableCollection<RowDefinition> RowDefinitions { get; } = new() {RowDefinition.Star(1)};
    public ObservableCollection<ColumnDefinition> ColumnDefinitions { get; } = new() {ColumnDefinition.Star(1)};

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

    public override Size GetRequestedSize()
        => WithCalculatedSize((columnWidths, rowHeights) =>
        {
            var width = 0;
            var height = 0;

            for (var i = 0; i < columnWidths.Length; i++)
            {
                width += columnWidths[i];
            }

            for (var i = 0; i < rowHeights.Length; i++)
            {
                height += rowHeights[i];
            }

            return new Size(width, height);
        }, new Option<Size>(new Size(0, 0), false));

    protected override void DefaultRenderer(Position position, Size size)
        => WithCalculatedSize((columnWidths, rowHeights) =>
        {
            foreach (var child in Children)
            {
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

                child.Render(new Position(position.X + left, position.Y + top), new Size(width, height));
            }
        }, new Option<Size>(size, true));

    private void WithCalculatedSize(WithSizes actionWithSizes, Option<Size> size)
    {
        WithCalculatedSize(Helper, size);

        object? Helper(Span<int> widths, Span<int> heights)
        {
            actionWithSizes(widths, heights);
            return null;
        }
    }

    private TResult WithCalculatedSize<TResult>(WithSizes<TResult> actionWithSizes, Option<Size> size)
    {
        //TODO: Optimize it, dont calculate all of these, only if there is Auto value(s)
        var columns = ColumnDefinitions.Count;
        var rows = RowDefinitions.Count;

        if (columns < 1) columns = 1;
        if (rows < 1) rows = 1;

        Span<int> allWidth = stackalloc int[columns * rows];
        Span<int> allHeight = stackalloc int[columns * rows];

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

        return actionWithSizes(columnWidths, rowHeights);
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
                var starValue = v.Length == 1 ? 1 : int.Parse(v[..^1]);
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
                var starValue = v.Length == 1 ? 1 : int.Parse(v[..^1]);
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
}