namespace TerminalUI.Styling;

public class Theme : ITheme
{
    public required IControlThemes ControlThemes { get; init; }
}