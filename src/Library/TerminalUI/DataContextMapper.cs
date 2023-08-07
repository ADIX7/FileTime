using System.ComponentModel;
using TerminalUI.Controls;

namespace TerminalUI;

public class DataContextMapper<T> : IDisposable
{
    private readonly IView<T> _source;
    private readonly Action<T?> _setter;

    public DataContextMapper(IView<T> source, Action<T?> setter)
    {
        ArgumentNullException.ThrowIfNull(source);

        _source = source;
        _setter = setter;
        source.PropertyChanged += SourceOnPropertyChanged;
    }

    private void SourceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IView<object>.DataContext)) return;
        _setter(_source.DataContext);
    }

    public void Dispose() => _source.PropertyChanged -= SourceOnPropertyChanged;
}