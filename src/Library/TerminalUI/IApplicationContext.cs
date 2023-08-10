using Microsoft.Extensions.Logging;
using TerminalUI.ConsoleDrivers;

namespace TerminalUI;

public interface IApplicationContext
{
    IEventLoop EventLoop { get; init; }
    bool IsRunning { get; set; }
    IConsoleDriver ConsoleDriver { get; init; }
    ILoggerFactory? LoggerFactory { get; init; }
    char EmptyCharacter { get; init; }
}