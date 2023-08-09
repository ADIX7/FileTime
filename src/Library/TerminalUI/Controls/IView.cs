using System.ComponentModel;
using TerminalUI.Models;
using TerminalUI.Traits;

namespace TerminalUI.Controls;

public interface IView : INotifyPropertyChanged, IDisposableCollection
{
    object? DataContext { get; set; }
    int? MinWidth { get; set; }
    int? MaxWidth { get; set; }
    int? Width { get; set; }
    int? MinHeight { get; set; }
    int? MaxHeight { get; set; }
    int? Height { get; set; }
    bool Attached { get; set; }
    Size GetRequestedSize();
    IApplicationContext? ApplicationContext { get; set; }
    List<object> Extensions { get; }
    
    Action<Position, Size> RenderMethod { get; set; }
    event Action<IView> Disposed;
    void Render(Position position, Size size);
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

    public TChild AddChild<TChild>(TChild child) where TChild : IView<T>;

    public TChild AddChild<TChild, TDataContext>(TChild child, Func<T?, TDataContext?> dataContextMapper)
        where TChild : IView<TDataContext>;
}