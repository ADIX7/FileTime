namespace TerminalUI.Color;

public interface IColor
{
    ColorType Type { get; }
    string ToConsoleColor();
    IColor AsForeground();
    IColor AsBackground();
}