using TerminalUI.Models;

namespace TerminalUI.ConsoleDrivers;

public interface IConsoleDriver
{
    void Init();
    void Dispose();
    void SetCursorPosition(Position position);
    void ResetColor();
    Position GetCursorPosition();
    void Write(string text);
    void Write(char text);
    bool CanRead();
    ConsoleKeyInfo ReadKey();
    void SetCursorVisible(bool cursorVisible);
    void SetForegroundColor(IColor foreground);
    void SetBackgroundColor(IColor background);
}