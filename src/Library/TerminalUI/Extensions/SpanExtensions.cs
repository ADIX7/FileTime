namespace TerminalUI.Extensions;

public static class SpanExtensions
{
    public static T GetFromMatrix<T>(this in Span<T> span, int x, int y, int width) => span[y * width + x];
    public static void SetToMatrix<T>(this in Span<T> span, T value, int x, int y, int width) => span[y * width + x] = value;
}