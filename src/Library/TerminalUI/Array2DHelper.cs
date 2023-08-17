using TerminalUI.ConsoleDrivers;
using TerminalUI.Models;

namespace TerminalUI;

public static class Array2DHelper
{
    public static void CombineArray2Ds<T, TResult>(T[,] array1, T[,] array2, Position array2Delta, TResult[,] array3, Func<T?, T?, TResult> func)
        where T : struct
        where TResult : struct
    {
        var array1Size = new Size(array1.GetLength(0), array1.GetLength(1));
        var array2Size = new Size(array2.GetLength(0), array2.GetLength(1));
        var array3Size = new Size(array3.GetLength(0), array3.GetLength(1));

        var maxX = int.Max(array1Size.Width, array2Size.Width + array2Delta.X);
        maxX = int.Max(maxX, array3Size.Width);

        var maxY = int.Max(array1Size.Height, array2Size.Height + array2Delta.Y);
        maxY = int.Max(maxY, array3Size.Height);


        for (var x = 0; x < maxX; x++)
        {
            for (var y = 0; y < maxY; y++)
            {
                if (x >= array3Size.Width
                    || y >= array3Size.Height) continue;

                T? v1 = x < array1Size.Width && y < array1Size.Height ? array1[x, y] : null;


                var array2X = x - array2Delta.X;
                var array2Y = y - array2Delta.Y;
                T? v2 = array2X >= 0
                        && array2X < array2Size.Width
                        && array2Y >= 0
                        && array2Y < array2Size.Height
                    ? array2[array2X, array2Y]
                    : null;

                array3[x, y] = func(v1, v2);
            }
        }
    }

    public static void RenderEmpty(
        IConsoleDriver driver,
        bool[,] updatedCells,
        bool[,] resultCells,
        char fillChar,
        Position position,
        Size size
    )
    {
        var endX = position.X + size.Width;
        var endY = position.Y + size.Height;
        for (var y = position.Y; y < endY; y++)
        {
            for (var x = position.X; x < endX; x++)
            {
                if (updatedCells[x, y]) continue;

                var startIndex = x;
                while (x < endX && !updatedCells[x, y])
                {
                    x++;
                }

                RenderEmpty(driver, resultCells, fillChar, startIndex, x, y);
            }
        }
    }

    private static void RenderEmpty(
        IConsoleDriver driver,
        bool[,] resultCells,
        char fillChar,
        int startX,
        int endX,
        int y
    )
    {
        var length = endX - startX;
        Span<char> text = stackalloc char[length];
        text.Fill(fillChar);

        driver.SetCursorPosition(new Position(startX, y));
        driver.Write(text);

        for (var x = startX; x < endX; x++)
        {
            resultCells[x, y] = true;
        }
    }
}