using TerminalUI.Color;
using TerminalUI.Models;
using ConsoleColor = TerminalUI.Color.ConsoleColor;

namespace TerminalUI.ConsoleDrivers;

public sealed class XTermDriver : DotnetDriver
{
    private Position _initialCursorPosition;

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
        Write("\x1b[?1047l");
        SetCursorPosition(_initialCursorPosition);
    }

    public override void SetBackgroundColor(IColor background)
    {
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