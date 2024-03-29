﻿using System.ComponentModel;

namespace TerminalUI.Color;

public readonly record struct Color256(byte Color, ColorType Type) : IColor
{
    public string ToConsoleColor()
        => Type switch
        {
            ColorType.Foreground => $"\x1b[38;5;{Color}m",
            ColorType.Background => $"\x1b[48;5;{Color}m",
            _ => throw new InvalidEnumArgumentException(nameof(Type), (int) Type, typeof(ColorType))
        };

    public IColor AsForeground() => this with {Type = ColorType.Foreground};

    public IColor AsBackground() => this with {Type = ColorType.Background};
}