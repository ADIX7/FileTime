using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TerminalUI.Controls;

public abstract class View<T> : IView<T>
{
    private readonly ConcurrentBag<IDisposable> _disposables = new();
    public T? DataContext { get; set; }
    public abstract void Render();

    public TChild CreateChild<TChild>() where TChild : IView<T>, new()
    {
        var child = new TChild
        {
            DataContext = DataContext
        };
        var mapper = new DataContextMapper<T>(this, d => child.DataContext = d);
        AddDisposable(mapper);
        child.AddDisposable(mapper);

        return child;
    }

    public TChild CreateChild<TChild, TDataContext>(Func<T?, TDataContext?> dataContextMapper) 
        where TChild : IView<TDataContext>, new()
    {
        var child = new TChild
        {
            DataContext = dataContextMapper(DataContext)
        };
        var mapper = new DataContextMapper<T>(this, d => child.DataContext = dataContextMapper(d));
        AddDisposable(mapper);
        child.AddDisposable(mapper);

        return child;
    }

    public void AddDisposable(IDisposable disposable) => _disposables.Add(disposable);

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetField<TProp>(ref TProp field, TProp value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<TProp>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this); // Violates rule
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            _disposables.Clear();
        }
    }
}