using System.Diagnostics;

namespace TerminalUI.Models;

[DebuggerDisplay("Left = {Left}, Top = {Top}, Right = {Right}, Bottom = {Bottom}")]
public record Thickness(int Left, int Top, int Right, int Bottom)
{
    public static implicit operator Thickness(int value) => new(value, value, value, value);
    public static implicit operator Thickness((int Left, int Top, int Right, int Bottom) value) => new(value.Left, value.Top, value.Right, value.Bottom);
    public static implicit operator Thickness(string s)
    {
        var parts = s.Split(' ');
        return parts.Length switch
        {
            1 => new Thickness(int.Parse(parts[0])),
            2 => new Thickness(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[0]), int.Parse(parts[1])),
            4 => new Thickness(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3])),
            _ => throw new ArgumentException("Invalid margin format", nameof(s))
        };
    }
}