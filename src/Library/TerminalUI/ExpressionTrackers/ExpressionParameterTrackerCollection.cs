using System.Collections.ObjectModel;

namespace TerminalUI.ExpressionTrackers;

public class ExpressionParameterTrackerCollection
{
    private readonly Dictionary<string, object?> _values = new();

    public ReadOnlyDictionary<string, object?> Values => new(_values);
    public event Action<string, object?>? ValueChanged;

    public void SetValue(string name, object? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        _values[name] = value;
        ValueChanged?.Invoke(name, value);
    }
}