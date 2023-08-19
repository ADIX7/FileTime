using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using TerminalUI.Controls;
using TerminalUI.Traits;

namespace TerminalUI;

public sealed class Binding<TDataContext, TExpressionResult, TResult> : PropertyTrackerBase<TDataContext, TExpressionResult>, IDisposable
{
    private readonly Func<TDataContext, TExpressionResult> _dataContextMapper;
    private IView<TDataContext> _dataSourceView;
    private object? _propertySource;
    private PropertyInfo _targetProperty;
    private readonly Func<TExpressionResult, TResult> _converter;
    private readonly TResult? _fallbackValue;
    private IDisposableCollection? _propertySourceDisposableCollection;
    private readonly string _parameterName;

    public Binding(
        IView<TDataContext> dataSourceView,
        Expression<Func<TDataContext?, TExpressionResult>> dataSourceExpression,
        object? propertySource,
        PropertyInfo targetProperty,
        Func<TExpressionResult, TResult> converter,
        TResult? fallbackValue = default
    ) : base(dataSourceExpression)
    {
        ArgumentNullException.ThrowIfNull(dataSourceView);
        ArgumentNullException.ThrowIfNull(dataSourceExpression);
        ArgumentNullException.ThrowIfNull(targetProperty);
        ArgumentNullException.ThrowIfNull(converter);

        _dataSourceView = dataSourceView;
        _dataContextMapper = dataSourceExpression.Compile();
        _propertySource = propertySource;
        _targetProperty = targetProperty;
        _converter = converter;
        _fallbackValue = fallbackValue;

        _parameterName = dataSourceExpression.Parameters[0].Name!;
        Parameters.SetValue(_parameterName, dataSourceView.DataContext);


        dataSourceView.PropertyChanged += View_PropertyChanged;
        Update(true);

        AddToSourceDisposables(propertySource);
    }

    private void AddToSourceDisposables(object? propertySource)
    {
        if (propertySource is IDisposableCollection propertySourceDisposableCollection)
        {
            _propertySourceDisposableCollection = propertySourceDisposableCollection;
            propertySourceDisposableCollection.AddDisposable(this);
        }
    }

    private void View_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IView<TDataContext>.DataContext)) return;

        Parameters.SetValue(_parameterName, _dataSourceView.DataContext);
        Update(true);
    }

    protected override void Update(bool couldCompute)
    {
        TResult? value;
        try
        {
            if (couldCompute)
            {
                value = _converter(_dataContextMapper(_dataSourceView.DataContext));
            }
            else
            {
                value = _fallbackValue;
            }
        }
        catch
        {
            value = _fallbackValue;
        }

        try
        {
            _targetProperty.SetValue(_propertySource, value);
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        //base.Dispose();
        _propertySourceDisposableCollection?.RemoveDisposable(this);
        _dataSourceView.RemoveDisposable(this);
        _dataSourceView.PropertyChanged -= View_PropertyChanged;
        _dataSourceView = null!;
        _propertySource = null!;
        _targetProperty = null!;
    }
}