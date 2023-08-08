using TerminalUI.Models;
using ConsoleColor = TerminalUI.Models.ConsoleColor;

namespace TerminalUI.ConsoleDrivers;

public class DotnetDriver : IConsoleDriver
{
    public virtual void Init() => Console.Clear();

    public void SetCursorPosition(Position position) => Console.SetCursorPosition(position.PosX, position.PosY);

    public void ResetColor() => Console.ResetColor();

    public Position GetCursorPosition()
    {
        var (x, y) = Console.GetCursorPosition();
        return new(x, y);
    }
    
    public void Write(string text) => Console.Write(text);

    public void Write(char text) => Console.Write(text);

    public virtual void Dispose() {}
    
    public bool CanRead() => Console.KeyAvailable;
    public ConsoleKeyInfo ReadKey() => Console.ReadKey(true);

    public void SetCursorVisible(bool cursorVisible) => Console.CursorVisible = cursorVisible;
    public virtual void SetForegroundColor(IColor foreground)
    {
        if (foreground is not ConsoleColor consoleColor) throw new NotSupportedException();
        Console.ForegroundColor = consoleColor.Color;
    }

    public virtual void SetBackgroundColor(IColor background)
    {
        if (background is not ConsoleColor consoleColor) throw new NotSupportedException();
        Console.ForegroundColor = consoleColor.Color;
    }
}