using System.ComponentModel;
using TerminalUI.Traits;

namespace TerminalUI.Controls;

public interface IView : INotifyPropertyChanged, IDisposableCollection
{
    object? DataContext { get; set; }
    Action RenderMethod { get; set; }
    IApplicationContext ApplicationContext { get; init;}
    event Action<IView> Disposed;
    event Action<IView> RenderRequested;
    void Render();
    void RequestRerender();
}

public interface IView<T> : IView
{
    new T? DataContext { get; set; }

    object? IView.DataContext
    {
        get => DataContext;
        set => DataContext = (T?) value;
    }

    TChild CreateChild<TChild>()
        where TChild : IView<T>, new();

    TChild CreateChild<TChild, TDataContext>(Func<T?, TDataContext?> dataContextMapper)
        where TChild : IView<TDataContext>, new();
}