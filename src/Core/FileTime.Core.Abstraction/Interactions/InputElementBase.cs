namespace FileTime.Core.Interactions;

public abstract class InputElementBase : IInputElement
{
    public InputType Type { get; }
    public string Label { get; }

    protected InputElementBase(string label, InputType type)
    {
        Label = label;
        Type = type;
    }
}