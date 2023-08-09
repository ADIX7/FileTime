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
    [Notify] private int? _minWidth;
    [Notify] private int? _maxWidth;
    [Notify] private int? _width;
    [Notify] private int? _minHeight;
    [Notify] private int? _maxHeight;
    [Notify] private int? _height;
    [Notify] private IApplicationContext? _applicationContext;
    private bool _attached;

    public bool Attached
    {
        get => _attached;
        set
        {
            if (_attached == value) return;
            _attached = value;
            if (value)
            {
                AttachChildren();
            }
        }
    }
    public List<object> Extensions { get; } = new();
    public Action<Position, Size> RenderMethod { get; set; }
    public event Action<IView>? Disposed;
    protected List<string> RerenderProperties { get; } = new();

    protected View()
    {
        RenderMethod = DefaultRenderer;
        ((INotifyPropertyChanged) this).PropertyChanged += Handle_PropertyChanged;
    }
    public abstract Size GetRequestedSize();

    protected virtual void AttachChildren()
    {
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

    protected abstract void DefaultRenderer(Position position, Size size);

    public void Render(Position position, Size size)
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

        RenderMethod(position, size);
    }

    public TChild CreateChild<TChild>() where TChild : IView<T>, new()
    {
        var child = new TChild();
        return AddChild(child);
    }

    public TChild CreateChild<TChild, TDataContext>(Func<T?, TDataContext?> dataContextMapper)
        where TChild : IView<TDataContext>, new()
    {
        var child = new TChild();
        return AddChild(child, dataContextMapper);
    }

    public virtual TChild AddChild<TChild>(TChild child) where TChild : IView<T>
    {
        child.DataContext = DataContext;
        child.ApplicationContext = ApplicationContext;

        var mapper = new DataContextMapper<T>(this, d => child.DataContext = d);
        AddDisposable(mapper);
        child.AddDisposable(mapper);

        return child;
    }

    public virtual TChild AddChild<TChild, TDataContext>(TChild child, Func<T?, TDataContext?> dataContextMapper)
        where TChild : IView<TDataContext>
    {
        child.DataContext = dataContextMapper(DataContext);
        child.ApplicationContext = ApplicationContext;

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