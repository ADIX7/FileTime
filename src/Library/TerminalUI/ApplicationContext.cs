using Microsoft.Extensions.Logging;
using TerminalUI.ConsoleDrivers;

namespace TerminalUI;

public class ApplicationContext : IApplicationContext
{
    public required IConsoleDriver ConsoleDriver { get; init; }
    public ILoggerFactory? LoggerFactory { get; init; }
    public IEventLoop EventLoop { get; init; }
    public bool IsRunning { get; set; }

    public ApplicationContext()
    {
        EventLoop = new EventLoop(this);
    }
}