namespace TerminalUI.Color;

public record ConsoleColor(System.ConsoleColor Color, ColorType Type) : IColor
{
    public string ToConsoleColor() => throw new NotImplementedException();
}