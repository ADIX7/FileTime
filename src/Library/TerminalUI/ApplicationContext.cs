using FileTime.App.Core.Models;
using Microsoft.Extensions.Logging;
using TerminalUI.ConsoleDrivers;

namespace TerminalUI;

public class ApplicationContext : IApplicationContext
{
    public required IConsoleDriver ConsoleDriver { get; init; }
    public required IFocusManager FocusManager { get; init; }
    public ILoggerFactory? LoggerFactory { get; init; }
    public IEventLoop EventLoop { get; init; }
    public bool IsRunning { get; set; }
    public char EmptyCharacter { get; init; } = ' ';

    public ApplicationContext()
    {
        EventLoop = new EventLoop(this);
    }
}