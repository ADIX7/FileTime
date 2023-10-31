using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TerminalUI.ConsoleDrivers;
using TerminalUI.Styling;

namespace TerminalUI;

public class ApplicationContext(IServiceProvider serviceProvider) : IApplicationContext
{
    private readonly Lazy<IConsoleDriver> _consoleDriver = new Lazy<IConsoleDriver>(serviceProvider.GetRequiredService<IConsoleDriver>);
    private readonly Lazy<IFocusManager> _focusManager = new Lazy<IFocusManager>(serviceProvider.GetRequiredService<IFocusManager>);
    private readonly Lazy<ILoggerFactory?> _loggerFactory = new Lazy<ILoggerFactory?>(serviceProvider.GetService<ILoggerFactory?>);
    private readonly Lazy<IRenderEngine> _renderEngine = new Lazy<IRenderEngine>(serviceProvider.GetRequiredService<IRenderEngine>);

    public IConsoleDriver ConsoleDriver => _consoleDriver.Value;
    public IFocusManager FocusManager => _focusManager.Value;
    public ILoggerFactory? LoggerFactory => _loggerFactory.Value;
    public IRenderEngine RenderEngine => _renderEngine.Value;
    public ITheme? Theme { get; set; }
    public bool IsRunning { get; set; }
    public char EmptyCharacter { get; init; } = ' ';
    public bool SupportUtf8Output { get; set; } = true;
}