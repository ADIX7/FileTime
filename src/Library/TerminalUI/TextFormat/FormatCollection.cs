using TerminalUI.ConsoleDrivers;

namespace TerminalUI.TextFormat;

public class FormatCollection : ITextFormat
{
    public List<ITextFormat> Formats { get; } = new();

    public Func<IReadOnlyList<ITextFormat>, TextFormatContext, bool> CanApplyPredicate { get; set; } = DefaultCanApplyPredicate;

    public bool CanApply(TextFormatContext context) => CanApplyPredicate(Formats, context);

    public void ApplyFormat(IConsoleDriver consoleDriver, TextFormatContext context)
    {
        foreach (var format in Formats)
        {
            format.ApplyFormat(consoleDriver, context);
        }
    }

    public static bool DefaultCanApplyPredicate(IReadOnlyList<ITextFormat> formats, TextFormatContext context) 
        => formats.Any(format => format.CanApply(context));
}