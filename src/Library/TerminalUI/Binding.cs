using System.ComponentModel;
using System.Reflection;
using TerminalUI.Controls;
using TerminalUI.Traits;

namespace TerminalUI;

public class Binding<TDataContext, TResult> : IDisposable
{
    private readonly Func<TDataContext, TResult> _dataContextMapper;
    private IView<TDataContext> _view;
    private object? _propertySource;
    private PropertyInfo _targetProperty;

    public Binding(
        IView<TDataContext> view, 
        Func<TDataContext, TResult> dataContextMapper, 
        object? propertySource, 
        PropertyInfo targetProperty
    )
    {
        _view = view;
        _dataContextMapper = dataContextMapper;
        _propertySource = propertySource;
        _targetProperty = targetProperty;
        view.PropertyChanged += View_PropertyChanged;
        _targetProperty.SetValue(_propertySource, _dataContextMapper(_view.DataContext));
        
        if(propertySource is IDisposableCollection disposableCollection)
            disposableCollection.AddDisposable(this);
        
        view.AddDisposable(this);
    }

    private void View_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IView<TDataContext>.DataContext)) return;

        _targetProperty.SetValue(_propertySource, _dataContextMapper(_view.DataContext));
    }

    public void Dispose()
    {
        _view.PropertyChanged -= View_PropertyChanged;
        _view = null!;
        _propertySource = null!;
        _targetProperty = null!;
    }
}