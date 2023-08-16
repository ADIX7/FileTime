namespace TerminalUI.Models;

public readonly struct SelectiveChar
{
    public readonly char Utf8Char;
    public readonly char AsciiChar;
    
    public SelectiveChar(char c)
    {
        Utf8Char = c;
        AsciiChar = c;
    }
    
    public SelectiveChar(char utf8Char, char asciiChar)
    {
        Utf8Char = utf8Char;
        AsciiChar = asciiChar;
    }
    
    public char GetChar(bool enableUtf8) => enableUtf8 ? Utf8Char : AsciiChar;
}