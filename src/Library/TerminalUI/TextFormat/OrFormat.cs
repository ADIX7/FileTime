using TerminalUI.ConsoleDrivers;

namespace TerminalUI.TextFormat;

public class OrFormat : ITextFormat
{
    public required ITextFormat Format1 { get; set; }
    public required ITextFormat Format2 { get; set; }

    public bool CanApply(TextFormatContext context) => true;

    public void ApplyFormat(IConsoleDriver consoleDriver, TextFormatContext context)
    {
        if (Format1.CanApply(context))
        {
            Format1.ApplyFormat(consoleDriver, context);
        }
        else
        {
            Format2.ApplyFormat(consoleDriver, context);
        }
    }
}