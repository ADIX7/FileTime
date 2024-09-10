using System.ComponentModel;

namespace TerminalUI.Color;

public readonly record struct Color256(byte? Color, ColorType Type) : IColor
{
    public string ToConsoleColor()
    {
        if (Color is null)
        {
            return string.Empty;
        }
        return Type switch
        {
            ColorType.Foreground => $"\x1b[38;5;{Color}m",
            ColorType.Background => $"\x1b[48;5;{Color}m",
            _ => throw new InvalidEnumArgumentException(nameof(Type), (int)Type, typeof(ColorType))
        };
    }

    public static Color256 DefaultForeground { get; } = new(null, ColorType.Foreground);
    public static Color256 DefaultBackground { get; } = new(null, ColorType.Background);
    private static Color256 DefaultForegroundAsBackground { get; } = new(7, ColorType.Background);
    private static Color256 DefaultBackgroundAsForeground { get; } = new(0, ColorType.Foreground);

    private static Dictionary<Color256, Color256> _defaultColorMapping = new()
    {
        { DefaultForeground, DefaultForegroundAsBackground },
        { DefaultBackground, DefaultBackgroundAsForeground },
        { DefaultForegroundAsBackground, DefaultForeground },
        { DefaultBackgroundAsForeground, DefaultBackground },
    };

    public IColor AsForeground()
        => _defaultColorMapping.TryGetValue(this, out var color)
            ? color
            : this with { Type = ColorType.Foreground };

    public IColor AsBackground()
        => _defaultColorMapping.TryGetValue(this, out var color)
            ? color
            : this with { Type = ColorType.Background };
}
