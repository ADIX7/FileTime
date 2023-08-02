using System.ComponentModel;
using PropertyChanged.SourceGenerator;

namespace FileTime.Core.Interactions;

public partial class OptionsInputElement<T> : InputElementBase, IOptionsInputElement, INotifyPropertyChanged
{
    public IReadOnlyCollection<OptionElement<T>> Options { get; }

    [Notify] private T? _value;

    IReadOnlyCollection<IOptionElement> IOptionsInputElement.Options => Options;

    object? IOptionsInputElement.Value
    {
        get => Options.FirstOrDefault(o => o.Value?.Equals(_value) ?? false);
        set
        {
            if (value is T newValue)
            {
                Value = newValue;
            }
            else if (value is OptionElement<T> optionElement)
            {
                Value = optionElement.Value;
            }
        }
    }

    public OptionsInputElement(string label, IEnumerable<OptionElement<T>> options) : base(label, InputType.Options)
    {
        Options = options.ToList();
    }
}