using System.ComponentModel;

namespace TerminalUI.Color;

public record struct ColorRgb(byte R, byte G, byte B, ColorType Type) : IColor
{
    public string ToConsoleColor()
        => Type switch
        {
            ColorType.Foreground => $"\x1b[38;2;{R};{G};{B};m",
            ColorType.Background => $"\x1b[48;2;{R};{G};{B};m",
            _ => throw new InvalidEnumArgumentException(nameof(Type), (int) Type, typeof(ColorType))
        };
    public IColor AsForeground() => this with {Type = ColorType.Foreground};

    public IColor AsBackground() => this with {Type = ColorType.Background};
}