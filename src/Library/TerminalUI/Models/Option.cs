namespace TerminalUI.Models;

public readonly ref struct Option<T>
{
    public readonly T Value;
    public readonly bool IsSome;

    public Option(T value, bool isSome)
    {
        Value = value;
        IsSome = isSome;
    }
}