using System.Collections.ObjectModel;

namespace TerminalUI.Controls;

public interface IChildContainer<T> : IView<T>
{
    ReadOnlyObservableCollection<IView> Children { get; }
}