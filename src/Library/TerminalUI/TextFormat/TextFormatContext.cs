namespace TerminalUI.TextFormat;

public readonly struct TextFormatContext(bool supportsAnsi)
{
    public readonly bool SupportsAnsi = supportsAnsi;
}