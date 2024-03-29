﻿using TerminalUI.Color;
using TerminalUI.Models;
using ConsoleColor = TerminalUI.Color.ConsoleColor;

namespace TerminalUI.ConsoleDrivers;

public sealed class XTermDriver : DotnetDriver
{
    private Position _initialCursorPosition;

    public XTermDriver()
    {
        SupportsAnsiEscapeSequence = true;
    }

    public override bool Init()
    {
        _initialCursorPosition = GetCursorPosition();
        Write("\x1b[?1047h");
        var isInitSuccessful = _initialCursorPosition == GetCursorPosition();
        if (isInitSuccessful)
        {
            Clear();
        }

        return isInitSuccessful;
    }

    public override void Dispose()
    {
        CheckThreadId = false;
        Write("\x1b[?1047l");
        SetCursorPosition(_initialCursorPosition);
    }

    public override void ResetStyle()
    {
        Write("\x1b[0m");
        base.ResetStyle();
    }

    public override void SetBackgroundColor(IColor background)
    {
        if (background == SpecialColor.None) return;

        if (background is ConsoleColor consoleColor)
        {
            Console.BackgroundColor = consoleColor.Color;
        }
        else
        {
            Write(background.ToConsoleColor());
        }
    }

    public override void SetForegroundColor(IColor foreground)
    {
        if (foreground == SpecialColor.None) return;

        if (foreground is ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor.Color;
        }
        else
        {
            Write(foreground.ToConsoleColor());
        }
    }
}