using TerminalUI.ConsoleDrivers;

namespace TerminalUI;

public interface IApplicationContext
{
    IEventLoop EventLoop { get; init; }
    bool IsRunning { get; set; }
    IConsoleDriver ConsoleDriver { get; init; }
}