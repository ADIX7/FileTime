namespace TerminalUI.Models;

public readonly ref struct Option<T>(T value, bool isSome)
{
    public readonly T Value = value;
    public readonly bool IsSome = isSome;
}