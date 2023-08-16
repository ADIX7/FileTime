using TerminalUI.ConsoleDrivers;

namespace TerminalUI.TextFormat;

public readonly struct EmptyFormat : ITextFormat
{
    public static EmptyFormat Instance => new();
    public bool CanApply(TextFormatContext context) => true;

    public void ApplyFormat(IConsoleDriver consoleDriver, TextFormatContext context)
    {
    }
}