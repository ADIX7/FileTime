using System.Collections.ObjectModel;
using System.ComponentModel;
using GeneralInputKey;
using TerminalUI.Color;
using TerminalUI.Models;
using TerminalUI.Traits;

namespace TerminalUI.Controls;

public delegate bool RenderMethod(in RenderContext renderContext, Position position, Size size);

public interface IView : INotifyPropertyChanged, IDisposableCollection
{
    object? DataContext { get; set; }
    int? MinWidth { get; set; }
    int? MaxWidth { get; set; }
    int? Width { get; set; }
    int ActualWidth { get; }
    int? MinHeight { get; set; }
    int? MaxHeight { get; set; }
    int? Height { get; set; }
    int ActualHeight { get; }
    Thickness Margin { get; set; }
    bool IsVisible { get; set; }
    bool Attached { get; set; }
    string? Name { get; set; }
    IColor? Foreground { get; set; }
    IColor? Background { get; set; }
    IApplicationContext? ApplicationContext { get; set; }
    List<object> Extensions { get; }
    RenderMethod RenderMethod { get; set; }
    IView? VisualParent { get; set; }
    ReadOnlyObservableCollection<IView> VisualChildren { get; }
    bool IsFocusBoundary { get; set; }
    event Action<IView> Disposed;

    Size GetRequestedSize();
    bool Render(in RenderContext renderContext, Position position, Size size);
    void HandleKeyInput(GeneralKeyEventArgs keyEventArgs);
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

    TChild AddChild<TChild>(TChild child) where TChild : IView<T>;

    TChild AddChild<TChild, TDataContext>(TChild child, Func<T?, TDataContext?> dataContextMapper)
        where TChild : IView<TDataContext>;

    void RemoveChild<TDataContext>(IView<TDataContext> child);
}