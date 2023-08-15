using Microsoft.Extensions.Logging;
using TerminalUI.ConsoleDrivers;
using TerminalUI.Styling;

namespace TerminalUI;

public interface IApplicationContext
{
    IRenderEngine RenderEngine { get; }
    bool IsRunning { get; set; }
    IConsoleDriver ConsoleDriver { get; }
    ILoggerFactory? LoggerFactory { get; }
    char EmptyCharacter { get; }
    IFocusManager FocusManager { get; }
    bool SupportUtf8Output { get; set; }
    ITheme? Theme { get; set; }
}