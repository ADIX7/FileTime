namespace TerminalUI.Models;

public readonly struct SelectiveChar(char utf8Char, char asciiChar)
{
    public readonly char Utf8Char = utf8Char;
    public readonly char AsciiChar = asciiChar;
    
    public SelectiveChar(char c) : this(c, c)
    {
    }

    public char GetChar(bool enableUtf8) => enableUtf8 ? Utf8Char : AsciiChar;
}