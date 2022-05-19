using System.ComponentModel;
using PropertyChanged.SourceGenerator;

namespace FileTime.Core.Interactions;

public partial class OptionsInputElement<T> : InputElementBase, IOptionsInputElement, INotifyPropertyChanged
{
    public IEnumerable<OptionElement<T>> Options { get; }

    [Notify] private T? _value;

    IEnumerable<IOptionElement> IOptionsInputElement.Options => Options;
    object? IOptionsInputElement.Value
    {
        get => Value;
        set => Value = (T?)value;
    }

    public OptionsInputElement(string label, IEnumerable<OptionElement<T>> options) : base(label, InputType.Options)
    {
        Options = options;
    }
}