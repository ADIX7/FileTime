namespace TerminalUI.Color;

public readonly record struct ConsoleColor(System.ConsoleColor Color, ColorType Type) : IColor
{
    public string ToConsoleColor() => throw new NotImplementedException();
    public IColor AsForeground() => this with {Type = ColorType.Foreground};

    public IColor AsBackground() => this with {Type = ColorType.Background};
}