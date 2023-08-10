namespace TerminalUI.Models;

public record Margin(int Left, int Top, int Right, int Bottom)
{
    public static implicit operator Margin(int value) => new(value, value, value, value);
    public static implicit operator Margin((int Left, int Top, int Right, int Bottom) value) => new(value.Left, value.Top, value.Right, value.Bottom);
    public static implicit operator Margin(string s)
    {
        var parts = s.Split(' ');
        return parts.Length switch
        {
            1 => new Margin(int.Parse(parts[0])),
            2 => new Margin(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[0]), int.Parse(parts[1])),
            4 => new Margin(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3])),
            _ => throw new ArgumentException("Invalid margin format", nameof(s))
        };
    }
}