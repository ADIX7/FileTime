using System.Linq.Expressions;

namespace TerminalUI;

public sealed class PropertyChangedHandler<TItem, TExpressionResult> : PropertyTrackerBase<TItem, TExpressionResult>, IDisposable
{
    private readonly TItem _dataSource;
    private readonly Action<string, bool, TExpressionResult?> _handler;
    private readonly PropertyTrackTreeItem? _propertyTrackTreeItem;
    private readonly Func<TItem, TExpressionResult> _propertyValueGenerator;

    public PropertyChangedHandler(
        TItem dataSource,
        Expression<Func<TItem, TExpressionResult>> dataSourceExpression,
        Action<string, bool, TExpressionResult?> handler
    ) : base(() => dataSource, dataSourceExpression)
    {
        _dataSource = dataSource;
        _handler = handler;
        ArgumentNullException.ThrowIfNull(dataSource);
        ArgumentNullException.ThrowIfNull(dataSourceExpression);
        ArgumentNullException.ThrowIfNull(handler);

        _propertyTrackTreeItem = CreateTrackingTree(dataSourceExpression);
        _propertyValueGenerator = dataSourceExpression.Compile();
        UpdateTrackers();
    }

    protected override void Update(string propertyPath)
    {
        TExpressionResult? value = default;
        var parsed = false;

        try
        {
            value = _propertyValueGenerator(_dataSource);
            parsed = true;
        }
        catch
        {
        }

        _handler(propertyPath, parsed, value);
    }
}