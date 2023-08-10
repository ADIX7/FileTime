using System.ComponentModel;
using TerminalUI.Controls;

namespace TerminalUI;

public class DataContextMapper<TSource, TTarget> : IDisposable
{
    private readonly Func<TSource?, TTarget?> _mapper;
    public IView<TSource> Source { get; }
    public IView<TTarget> Target { get; }

    public DataContextMapper(IView<TSource> source, IView<TTarget> target, Func<TSource?, TTarget?> mapper)
    {
        _mapper = mapper;
        ArgumentNullException.ThrowIfNull(source);

        Source = source;
        Target = target;
        source.PropertyChanged += SourceOnPropertyChanged;
    }

    private void SourceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IView<object>.DataContext)) return;
        Target.DataContext = _mapper(Source.DataContext);
    }

    public void Dispose()
    {
        Source.PropertyChanged -= SourceOnPropertyChanged;
        Source.RemoveDisposable(this);
        Target.RemoveDisposable(this);
    }
}