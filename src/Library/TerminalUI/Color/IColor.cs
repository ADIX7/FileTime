namespace TerminalUI.Color;

public interface IColor
{
    ColorType Type { get; }
    string ToConsoleColor();
}