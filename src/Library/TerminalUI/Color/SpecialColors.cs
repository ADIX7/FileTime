namespace TerminalUI.Color;

public class SpecialColor : IColor
{
    private SpecialColor(){}
    public ColorType Type => ColorType.Unknown;
    public string ToConsoleColor() => throw new NotImplementedException();

    public IColor AsForeground() => this;

    public IColor AsBackground() => this;

    public static SpecialColor None { get; } = new();
}