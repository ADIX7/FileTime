namespace TerminalUI.TextFormat;

public readonly struct TextFormatContext
{
    public readonly bool SupportsAnsi;

    public TextFormatContext(bool supportsAnsi)
    {
        SupportsAnsi = supportsAnsi;
    }
}