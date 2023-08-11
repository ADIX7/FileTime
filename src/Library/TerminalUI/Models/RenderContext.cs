using System.Diagnostics;
using TerminalUI.Color;
using TerminalUI.ConsoleDrivers;

namespace TerminalUI.Models;

[DebuggerDisplay("RenderId = {RenderId}, ForceRerender = {ForceRerender}, Driver = {ConsoleDriver.GetType().Name}")]
public readonly ref struct RenderContext
{
    private static int _renderId;
    public readonly int RenderId;
    public readonly IConsoleDriver ConsoleDriver;
    public readonly bool ForceRerender;
    public readonly IColor? Foreground;
    public readonly IColor? Background;

    public RenderContext(
        IConsoleDriver consoleDriver, 
        bool forceRerender, 
        IColor? foreground, 
        IColor? background)
    {
        RenderId = _renderId++;
        
        ConsoleDriver = consoleDriver;
        ForceRerender = forceRerender;
        Foreground = foreground;
        Background = background;
    }
    
    public static RenderContext Empty => new(null!, false, null, null);
}