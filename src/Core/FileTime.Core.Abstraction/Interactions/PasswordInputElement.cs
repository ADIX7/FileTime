using System.ComponentModel;
using PropertyChanged.SourceGenerator;

namespace FileTime.Core.Interactions;

public partial class PasswordInputElement : InputElementBase, INotifyPropertyChanged
{
    public char PasswordChar { get; }

    [Notify] private string? _value;

    public PasswordInputElement(string label, string? value = null, char passwordChar = '*') : base(label, InputType.Password)
    {
        PasswordChar = passwordChar;
        _value = value;
    }
}