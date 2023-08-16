using System.Diagnostics;
using TerminalUI.Color;
using TerminalUI.ConsoleDrivers;
using TerminalUI.TextFormat;

namespace TerminalUI.Models;

[DebuggerDisplay("RenderId = {RenderId}, ForceRerender = {ForceRerender}, Driver = {ConsoleDriver.GetType().Name}")]
public readonly ref struct RenderContext
{
    private static int _renderId;
    public int RenderId { get; init; }
    public IConsoleDriver ConsoleDriver { get; init; }
    public bool ForceRerender { get; init; }
    public IColor? Foreground { get; init; }
    public IColor? Background { get; init; }
    public RenderStatistics Statistics { get; init; }
    public TextFormatContext TextFormat { get; init; }

    public RenderContext(
        IConsoleDriver consoleDriver,
        bool forceRerender,
        IColor? foreground,
        IColor? background,
        RenderStatistics statistics,
        TextFormatContext textFormat)
    {
        RenderId = _renderId++;

        ConsoleDriver = consoleDriver;
        ForceRerender = forceRerender;
        Foreground = foreground;
        Background = background;
        Statistics = statistics;
        TextFormat = textFormat;
    }

    public static RenderContext Empty =>
        new(
            null!,
            false,
            null,
            null,
            new RenderStatistics(),
            new TextFormatContext(false)
        );
}