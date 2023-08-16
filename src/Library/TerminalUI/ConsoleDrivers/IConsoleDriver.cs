using TerminalUI.Color;
using TerminalUI.Models;

namespace TerminalUI.ConsoleDrivers;

public interface IConsoleDriver
{
    bool SupportsAnsiEscapeSequence { get; }
    bool Init();
    void Dispose();
    void SetCursorPosition(Position position);
    void ResetColor();
    void ResetStyle();
    Position GetCursorPosition();
    void Write(string text);
    void Write(ReadOnlySpan<char> text);
    void Write(char text);
    bool CanRead();
    ConsoleKeyInfo ReadKey();
    void SetCursorVisible(bool cursorVisible);
    void SetForegroundColor(IColor foreground);
    void SetBackgroundColor(IColor background);
    Size GetWindowSize();
    void Clear();
}