using FileTime.App.Core.Models;
using Microsoft.Extensions.Logging;
using TerminalUI.ConsoleDrivers;

namespace TerminalUI;

public interface IApplicationContext
{
    IRenderEngine RenderEngine { get; }
    bool IsRunning { get; set; }
    IConsoleDriver ConsoleDriver { get; }
    ILoggerFactory? LoggerFactory { get; }
    char EmptyCharacter { get; }
    IFocusManager FocusManager { get; }
}