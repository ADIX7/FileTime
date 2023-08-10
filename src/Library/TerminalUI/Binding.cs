using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using TerminalUI.Controls;
using TerminalUI.Traits;

namespace TerminalUI;

public class Binding<TDataContext, TExpressionResult, TResult> : IDisposable
{
    private readonly Func<TDataContext, TExpressionResult> _dataContextMapper;
    private IView<TDataContext> _dataSourceView;
    private object? _propertySource;
    private PropertyInfo _targetProperty;
    private readonly Func<TExpressionResult, TResult> _converter;
    private readonly TResult? _fallbackValue;
    private IDisposableCollection? _propertySourceDisposableCollection;
    private PropertyTrackTreeItem? _propertyTrackTreeItem;
    private IPropertyChangeTracker? _propertyChangeTracker;

    public Binding(
        IView<TDataContext> dataSourceView,
        Expression<Func<TDataContext?, TExpressionResult>> dataSourceExpression,
        object? propertySource,
        PropertyInfo targetProperty,
        Func<TExpressionResult, TResult> converter,
        TResult? fallbackValue = default
    )
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

        InitTrackingTree(dataSourceExpression);

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

    private void InitTrackingTree(Expression<Func<TDataContext?, TExpressionResult>> dataContextExpression)
    {
        var properties = new List<string>();
        FindReactiveProperties(dataContextExpression, properties);

        if (properties.Count > 0)
        {
            var rootItem = new PropertyTrackTreeItem();
            foreach (var property in properties)
            {
                var pathParts = property.Split('.');
                var currentItem = rootItem;
                for (var i = 0; i < pathParts.Length; i++)
                {
                    if (!currentItem.Children.TryGetValue(pathParts[i], out var child))
                    {
                        child = new PropertyTrackTreeItem();
                        currentItem.Children.Add(pathParts[i], child);
                    }

                    currentItem = child;
                }
            }

            _propertyTrackTreeItem = rootItem;
        }
    }

    private string? FindReactiveProperties(Expression? expression, List<string> properties)
    {
        if (expression is null) return "";
        
        if (expression is LambdaExpression lambdaExpression)
        {
            SavePropertyPath(FindReactiveProperties(lambdaExpression.Body, properties));
        }
        else if (expression is ConditionalExpression conditionalExpression)
        {
            SavePropertyPath(FindReactiveProperties(conditionalExpression.Test, properties));
            SavePropertyPath(FindReactiveProperties(conditionalExpression.IfTrue, properties));
            SavePropertyPath(FindReactiveProperties(conditionalExpression.IfFalse, properties));
        }
        else if (expression is MemberExpression memberExpression)
        {
            if (memberExpression.Expression is not null)
            {
                FindReactiveProperties(memberExpression.Expression, properties);

                if (FindReactiveProperties(memberExpression.Expression, properties) is { } path
                    && memberExpression.Member is PropertyInfo dataContextPropertyInfo)
                {
                    path += "." + memberExpression.Member.Name;
                    return path;
                }
            }
        }
        else if (expression is MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Object is
                {
                    NodeType:
                    not ExpressionType.Parameter
                    and not ExpressionType.Constant
                } methodObject)
            {
                SavePropertyPath(FindReactiveProperties(methodObject, properties));
            }

            foreach (var argument in methodCallExpression.Arguments)
            {
                SavePropertyPath(FindReactiveProperties(argument, properties));
            }
        }
        else if (expression is BinaryExpression binaryExpression)
        {
            SavePropertyPath(FindReactiveProperties(binaryExpression.Left, properties));
            SavePropertyPath(FindReactiveProperties(binaryExpression.Right, properties));
        }
        else if (expression is UnaryExpression unaryExpression)
        {
            SavePropertyPath(FindReactiveProperties(unaryExpression.Operand, properties));
        }
        else if (expression is ParameterExpression parameterExpression)
        {
            if (parameterExpression.Type == typeof(TDataContext))
            {
                return "";
            }
        }

        return null;

        void SavePropertyPath(string? path)
        {
            if (path is null) return;
            path = path.TrimStart('.');
            properties.Add(path);
        }
    }

    private void View_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IView<TDataContext>.DataContext)) return;

        UpdateTrackers();
        UpdateTargetProperty();
    }

    private void UpdateTrackers()
    {
        if (_propertyChangeTracker is not null)
        {
            _propertyChangeTracker.Dispose();
        }

        if (_propertyTrackTreeItem is not null)
        {
            _propertyChangeTracker = PropertyChangeHelper.TraverseDataContext(
                _propertyTrackTreeItem,
                _dataSourceView.DataContext,
                UpdateTargetProperty
            );
        }
    }

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