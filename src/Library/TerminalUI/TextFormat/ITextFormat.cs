using TerminalUI.ConsoleDrivers;

namespace TerminalUI.TextFormat;

public interface ITextFormat
{
    bool CanApply(TextFormatContext context);
    void ApplyFormat(IConsoleDriver consoleDriver, TextFormatContext context);
}