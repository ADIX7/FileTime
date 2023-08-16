﻿using TerminalUI.Color;
using TerminalUI.Models;
using ConsoleColor = TerminalUI.Color.ConsoleColor;

namespace TerminalUI.ConsoleDrivers;

public class DotnetDriver : IConsoleDriver
{
    public bool SupportsAnsiEscapeSequence { get; protected set; }

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

    public void Write(string text) => Console.Out.Write(text);
    public void Write(ReadOnlySpan<char> text) => Console.Out.Write(text);

    public void Write(char text) => Console.Out.Write(text);

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