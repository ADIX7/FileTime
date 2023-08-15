using TerminalUI.Styling.Controls;

namespace TerminalUI.Styling;

public class ControlThemes : IControlThemes
{
    public required IProgressBarTheme ProgressBar { get; init; }
}