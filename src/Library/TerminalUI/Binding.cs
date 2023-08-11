using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using TerminalUI.Controls;
using TerminalUI.Traits;

namespace TerminalUI;

public sealed class Binding<TDataContext, TExpressionResult, TResult> : PropertyTrackerBase<TDataContext, TExpressionResult>
{
    private readonly Func<TDataContext, TExpressionResult> _dataContextMapper;
    private IView<TDataContext> _dataSourceView;
    private object? _propertySource;
    private PropertyInfo _targetProperty;
    private readonly Func<TExpressionResult, TResult> _converter;
    private readonly TResult? _fallbackValue;
    private IDisposableCollection? _propertySourceDisposableCollection;

    public Binding(
        IView<TDataContext> dataSourceView,
        Expression<Func<TDataContext?, TExpressionResult>> dataSourceExpression,
        object? propertySource,
        PropertyInfo targetProperty,
        Func<TExpressionResult, TResult> converter,
        TResult? fallbackValue = default
    ) : base(() => dataSourceView.DataContext, dataSourceExpression)
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

        UpdateTrackers();

        dataSourceView.PropertyChanged += View_PropertyChanged;
        UpdateTargetProperty();

        AddToSourceDisposables(propertySource);

        dataSourceView.AddDisposable(this);
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

        UpdateTrackers();
        UpdateTargetProperty();
    }

    protected override void Update(string propertyPath) => UpdateTargetProperty();

    private void UpdateTargetProperty()
    {
        TResult value;
        try
        {
            value = _converter(_dataContextMapper(_dataSourceView.DataContext));
        }
        catch
        {
            value = _fallbackValue;
        }

        _targetProperty.SetValue(_propertySource, value);
    }

    public override void Dispose()
    {
        base.Dispose();
        _propertySourceDisposableCollection?.RemoveDisposable(this);
        _dataSourceView.RemoveDisposable(this);
        _dataSourceView.PropertyChanged -= View_PropertyChanged;
        _dataSourceView = null!;
        _propertySource = null!;
        _targetProperty = null!;
    }
}