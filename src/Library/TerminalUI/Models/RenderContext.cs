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
    public readonly RenderStatistics Statistics;

    public RenderContext(
        IConsoleDriver consoleDriver,
        bool forceRerender,
        IColor? foreground,
        IColor? background,
        RenderStatistics statistics)
    {
        RenderId = _renderId++;

        ConsoleDriver = consoleDriver;
        ForceRerender = forceRerender;
        Foreground = foreground;
        Background = background;
        Statistics = statistics;
    }

    public static RenderContext Empty =>
        new(
            null!,
            false,
            null,
            null,
            new RenderStatistics()
        );
}