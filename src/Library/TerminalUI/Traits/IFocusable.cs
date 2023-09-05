using TerminalUI.ConsoleDrivers;
using TerminalUI.Controls;

namespace TerminalUI.Traits;

public interface IFocusable : IView
{
    void Focus();
    void UnFocus();
    void SetCursorPosition(IConsoleDriver consoleDriver);
}