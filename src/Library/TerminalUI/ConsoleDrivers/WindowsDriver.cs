using TerminalUI.Models;

namespace TerminalUI.ConsoleDrivers;

public sealed class WindowsDriver : DotnetDriver
{
    public override void Init() => Console.Out.Write("\x1b[?1049h");

    public override void Dispose() => Console.Out.Write("\x1b[?1049l");

    public override void SetBackgroundColor(IColor background) 
        => Write(background.ToConsoleColor());

    public override void SetForegroundColor(IColor foreground)
        => Write(foreground.ToConsoleColor());
}