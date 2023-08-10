using TerminalUI.ConsoleDrivers;

namespace TerminalUI.Models;

public readonly ref struct RenderContext
{
    private static int _renderId = 0;
    public readonly int RenderId;
    public readonly IConsoleDriver ConsoleDriver;

    public RenderContext(IConsoleDriver consoleDriver)
    {
        ConsoleDriver = consoleDriver;
        RenderId = _renderId++;
    }
    
    public static RenderContext Empty => new(null!);
}