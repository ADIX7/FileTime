using PropertyChanged.SourceGenerator;

namespace FileTime.Core.Interactions;

public partial class TextInputElement : InputElementBase
{
    [Notify] private string? _value;
    private readonly Action<string?>? _update;

    public TextInputElement(
        string label, 
        string? value = null,
        Action<string?>? update = null) : base(label, InputType.Text)
    {
        _value = value;
        _update = update;
    }
    
    private void OnValueChanged(string? oldValue, string? newValue) => _update?.Invoke(newValue);
}