using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using TerminalUI.Controls;
using TerminalUI.Traits;

namespace TerminalUI;

public class Binding<TDataContext, TResult> : IDisposable
{
    private readonly Func<TDataContext, TResult> _dataContextMapper;
    private IView<TDataContext> _dataSourceView;
    private object? _propertySource;
    private PropertyInfo _targetProperty;
    private readonly List<string> _rerenderProperties;
    private readonly IDisposableCollection? _propertySourceDisposableCollection;
    private INotifyPropertyChanged? _dataSourceLastDataContext;

    public Binding(
        IView<TDataContext> dataSourceView,
        Expression<Func<TDataContext?, TResult>> dataContextExpression,
        object? propertySource,
        PropertyInfo targetProperty,
        IEnumerable<string>? rerenderProperties = null
    )
    {
        ArgumentNullException.ThrowIfNull(dataSourceView);
        ArgumentNullException.ThrowIfNull(dataContextExpression);
        ArgumentNullException.ThrowIfNull(targetProperty);
        _dataSourceView = dataSourceView;
        _dataContextMapper = dataContextExpression.Compile();
        _propertySource = propertySource;
        _targetProperty = targetProperty;
        _rerenderProperties = rerenderProperties?.ToList() ?? new List<string>();

        FindReactiveProperties(dataContextExpression);

        dataSourceView.PropertyChanged += View_PropertyChanged;
        var initialValue = _dataContextMapper(_dataSourceView.DataContext);
        _targetProperty.SetValue(_propertySource, initialValue);

        if (propertySource is IDisposableCollection propertySourceDisposableCollection)
        {
            propertySourceDisposableCollection.AddDisposable(this);
            _propertySourceDisposableCollection = propertySourceDisposableCollection;
        }

        if (_dataSourceView.DataContext is INotifyPropertyChanged dataSourcePropertyChanged)
        {
            _dataSourceLastDataContext = dataSourcePropertyChanged;
            dataSourcePropertyChanged.PropertyChanged += DataContext_PropertyChanged;
        }

        dataSourceView.AddDisposable(this);
    }

    private void FindReactiveProperties(Expression expression)
    {
        if (expression is LambdaExpression lambdaExpression)
        {
            FindReactiveProperties(lambdaExpression.Body);
        }
        else if (expression is ConditionalExpression conditionalExpression)
        {
            FindReactiveProperties(conditionalExpression.IfFalse);
            FindReactiveProperties(conditionalExpression.IfTrue);
        }
        else if (expression is MemberExpression {Member: PropertyInfo dataContextPropertyInfo})
        {
            _rerenderProperties.Add(dataContextPropertyInfo.Name);
        }
        //TODO: Handle other expression types
    }

    private void View_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IView<TDataContext>.DataContext)) return;

        if (_dataSourceLastDataContext is not null)
        {
            _dataSourceLastDataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

        if (_dataSourceView.DataContext is INotifyPropertyChanged dataSourcePropertyChanged)
        {
            _dataSourceLastDataContext = dataSourcePropertyChanged;
            dataSourcePropertyChanged.PropertyChanged += DataContext_PropertyChanged;
        }

        UpdateTargetProperty();
    }

    private void DataContext_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == null
            || !_rerenderProperties.Contains(e.PropertyName)) return;
        UpdateTargetProperty();
    }

    private void UpdateTargetProperty()
        => _targetProperty.SetValue(_propertySource, _dataContextMapper(_dataSourceView.DataContext));

    public void Dispose()
    {
        _propertySourceDisposableCollection?.RemoveDisposable(this);
        _dataSourceView.RemoveDisposable(this);
        _dataSourceView.PropertyChanged -= View_PropertyChanged;
        _dataSourceView = null!;
        _propertySource = null!;
        _targetProperty = null!;
    }
}