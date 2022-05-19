using System.ComponentModel;
using PropertyChanged.SourceGenerator;

namespace FileTime.Core.Interactions;

public partial class TextInputElement : InputElementBase, INotifyPropertyChanged
{
    [Notify] private string? _value;

    public TextInputElement(string label, string? value = null) : base(label, InputType.Text)
    {
        _value = value;
    }
}