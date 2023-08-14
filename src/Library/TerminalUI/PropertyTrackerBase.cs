using System.Linq.Expressions;
using System.Reflection;

namespace TerminalUI;

public abstract class PropertyTrackerBase<TSource, TExpressionResult> : IDisposable
{
    private readonly Func<TSource?> _source;
    protected PropertyTrackTreeItem? PropertyTrackTreeItem { get; }
    protected IPropertyChangeTracker? PropertyChangeTracker { get; private set; }

    protected PropertyTrackerBase(
        Func<TSource?> source,
        Expression<Func<TSource?, TExpressionResult>> dataSourceExpression)
    {
        ArgumentNullException.ThrowIfNull(dataSourceExpression);

        _source = source;
        PropertyTrackTreeItem = CreateTrackingTree(dataSourceExpression);
    }

    protected PropertyTrackTreeItem? CreateTrackingTree(Expression<Func<TSource?, TExpressionResult>> dataContextExpression)
    {
        var properties = new List<string>();
        FindReactiveProperties(dataContextExpression, properties);

        if (properties.Count > 0)
        {
            var rootItem = new PropertyTrackTreeItem(null!);
            foreach (var property in properties)
            {
                var pathParts = property.Split('.');
                var currentItem = rootItem;
                for (var i = 0; i < pathParts.Length; i++)
                {
                    if (!currentItem.Children.TryGetValue(pathParts[i], out var child))
                    {
                        child = new PropertyTrackTreeItem(pathParts[i]);
                        currentItem.Children.Add(pathParts[i], child);
                    }

                    currentItem = child;
                }
            }

            return rootItem;
        }

        return null;
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
            return FindReactiveProperties(unaryExpression.Operand, properties);
        }
        else if (expression is ParameterExpression parameterExpression)
        {
            if (parameterExpression.Type == typeof(TSource))
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

    protected void UpdateTrackers()
    {
        if (PropertyChangeTracker is not null)
        {
            PropertyChangeTracker.Dispose();
        }

        if (PropertyTrackTreeItem is not null)
        {
            PropertyChangeTracker = PropertyChangeHelper.CreatePropertyTracker(
                null,
                PropertyTrackTreeItem,
                _source(),
                Update
            );
        }
    }

    protected abstract void Update(string propertyPath);

    public virtual void Dispose() => PropertyChangeTracker?.Dispose();
}