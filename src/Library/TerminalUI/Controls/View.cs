using System.Buffers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged.SourceGenerator;
using TerminalUI.Models;

namespace TerminalUI.Controls;

public abstract partial class View<T> : IView<T>
{
    private readonly List<IDisposable> _disposables = new();
    [Notify] private T? _dataContext;
    public Action<Position> RenderMethod { get; set; }
    public IApplicationContext? ApplicationContext { get; init; }
    public event Action<IView>? Disposed;
    protected List<string> RerenderProperties { get; } = new();

    protected View()
    {
        RenderMethod = DefaultRenderer;
        ((INotifyPropertyChanged) this).PropertyChanged += Handle_PropertyChanged;
    }

    private void Handle_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not null 
            && (e.PropertyName == nameof(IView.DataContext) 
                || RerenderProperties.Contains(e.PropertyName)
            )
        )
        {
            ApplicationContext?.EventLoop.RequestRerender();
        }
    }

    protected abstract void DefaultRenderer(Position position);

    public void Render(Position position)
    {
        if (RenderMethod is null)
        {
            throw new NullReferenceException(
                nameof(RenderMethod)
                + " is null, cannot render content of "
                + GetType().Name
                + " with DataContext of "
                + DataContext?.GetType().Name);
        }

        RenderMethod(position);
    }

    public TChild CreateChild<TChild>() where TChild : IView<T>, new()
    {
        var child = new TChild
        {
            DataContext = DataContext,
            ApplicationContext = ApplicationContext
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
            DataContext = dataContextMapper(DataContext),
            ApplicationContext = ApplicationContext
        };
        var mapper = new DataContextMapper<T>(this, d => child.DataContext = dataContextMapper(d));
        AddDisposable(mapper);
        child.AddDisposable(mapper);

        return child;
    }

    public void AddDisposable(IDisposable disposable) => _disposables.Add(disposable);
    public void RemoveDisposable(IDisposable disposable) => _disposables.Remove(disposable);

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            var arrayPool = ArrayPool<IDisposable>.Shared;
            var disposablesCount = _disposables.Count;
            var disposables = arrayPool.Rent(disposablesCount);
            _disposables.CopyTo(disposables);
            for (var i = 0; i < disposablesCount; i++)
            {
                disposables[i].Dispose();
            }

            arrayPool.Return(disposables, true);

            _disposables.Clear();
            Disposed?.Invoke(this);
        }
    }
}