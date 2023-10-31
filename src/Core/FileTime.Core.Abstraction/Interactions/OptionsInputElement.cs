using System.ComponentModel;
using PropertyChanged.SourceGenerator;

namespace FileTime.Core.Interactions;

public partial class OptionsInputElement<T>(string label, IEnumerable<OptionElement<T>> options) : InputElementBase(label, InputType.Options), IOptionsInputElement, INotifyPropertyChanged
{
    public IReadOnlyCollection<OptionElement<T>> Options { get; } = Enumerable.ToList<OptionElement<T>>(options);

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
}