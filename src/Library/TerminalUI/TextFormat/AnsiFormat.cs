using TerminalUI.ConsoleDrivers;

namespace TerminalUI.TextFormat;

public class AnsiFormat : ITextFormat
{
    public bool IsUnderline { get; set; }
    public bool IsItalic { get; set; }
    public bool IsBold { get; set; }
    public bool CanApply(TextFormatContext context) => context.SupportsAnsi;

    public void ApplyFormat(IConsoleDriver consoleDriver, TextFormatContext context)
    {
        if (!context.SupportsAnsi) return;
        
        if (IsUnderline)
            consoleDriver.Write("\x01b[4m");

        if (IsItalic)
            consoleDriver.Write("\x01b[3m");

        if (IsBold)
            consoleDriver.Write("\x01b[1m");
    }
}