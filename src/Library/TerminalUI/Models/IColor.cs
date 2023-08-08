namespace TerminalUI.Models;

public interface IColor
{
    ColorType Type { get; }
    string ToConsoleColor();
}