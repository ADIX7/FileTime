using System.ComponentModel;
using TerminalUI.Traits;

namespace TerminalUI.Controls;

public interface IView<T> : INotifyPropertyChanged, IDisposableCollection
{
    T? DataContext { get; set; }
    void Render();

    TChild CreateChild<TChild>()
        where TChild : IView<T>, new();

    TChild CreateChild<TChild, TDataContext>(Func<T?, TDataContext?> dataContextMapper)
        where TChild : IView<TDataContext>, new();
}