using TerminalUI.Color;
using TerminalUI.ConsoleDrivers;

namespace TerminalUI.TextFormat;

public class SimpleFormat : ITextFormat
{
    public IColor? Foreground { get; init; }
    public IColor? Background { get; init; }
    public bool CanApply(TextFormatContext context) => true;

    public void ApplyFormat(IConsoleDriver consoleDriver, TextFormatContext context)
    {
        if (Foreground is not null)
            consoleDriver.SetForegroundColor(Foreground);

        if (Background is not null)
            consoleDriver.SetBackgroundColor(Background);
    }
}