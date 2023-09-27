namespace FileTime.Core.Interactions;

public class OptionElement<T> : IOptionElement
{
    public string Text { get; }
    public T Value { get; }

    object? IOptionElement.Value => Value;

    public OptionElement(string text, T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        Text = text;
        Value = value;
    }
}