using TerminalUI.Color;
using TerminalUI.Models;
using ConsoleColor = TerminalUI.Color.ConsoleColor;

namespace TerminalUI.ConsoleDrivers;

public class DotnetDriver : IConsoleDriver
{
    protected bool CheckThreadId;
    public bool SupportsAnsiEscapeSequence { get; protected set; }
    public int ThreadId { get; set; }

    public void EnterRestrictedMode() => CheckThreadId = true;

    public virtual bool Init()
    {
        Console.Clear();
        return true;
    }

    public void SetCursorPosition(Position position) => Console.SetCursorPosition(position.X, position.Y);

    public virtual void ResetColor() => Console.ResetColor();
    public virtual void ResetStyle() => Console.ResetColor();

    public Position GetCursorPosition()
    {
        var (x, y) = Console.GetCursorPosition();
        return new(x, y);
    }

    public void Write(string text)
    {
        CheckThread();
        Console.Out.Write(text);
    }

    public void Write(ReadOnlySpan<char> text)
    {
        CheckThread();
        Console.Out.Write(text);
    }

    public void Write(char text)
    {
        CheckThread();
        Console.Out.Write(text);
    }

    private void CheckThread()
    {
        if (CheckThreadId && ThreadId != Thread.CurrentThread.ManagedThreadId)
        {
            throw new InvalidOperationException("Cannot write to console from another thread");
        }
    }

    public virtual void Dispose() => Console.Clear();

    public bool CanRead() => Console.KeyAvailable;
    public ConsoleKeyInfo ReadKey() => Console.ReadKey(true);

    public void SetCursorVisible(bool cursorVisible) => Console.CursorVisible = cursorVisible;

    public virtual void SetForegroundColor(IColor foreground)
    {
        if (foreground == SpecialColor.None) return;

        if (foreground is not ConsoleColor consoleColor) throw new NotSupportedException();
        Console.ForegroundColor = consoleColor.Color;
    }

    public virtual void SetBackgroundColor(IColor background)
    {
        if (background == SpecialColor.None) return;

        if (background is not ConsoleColor consoleColor) throw new NotSupportedException();
        Console.BackgroundColor = consoleColor.Color;
    }

    public Size GetWindowSize() => new(Console.WindowWidth, Console.WindowHeight);
    public void Clear() => Console.Clear();
}