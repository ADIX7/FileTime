using System.Linq.Expressions;

namespace TerminalUI;

public sealed class PropertyChangedHandler<TItem, TExpressionResult> : PropertyTrackerBase<TItem, TExpressionResult>
{
    private readonly TItem _dataSource;
    private readonly Action<bool, TExpressionResult?> _handler;
    private readonly Func<TItem, TExpressionResult> _propertyValueGenerator;

    public PropertyChangedHandler(
        TItem dataSource,
        Expression<Func<TItem, TExpressionResult>> dataSourceExpression,
        Action<bool, TExpressionResult?> handler
    ) : base(dataSourceExpression!)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        ArgumentNullException.ThrowIfNull(dataSourceExpression);
        ArgumentNullException.ThrowIfNull(handler);
        
        _dataSource = dataSource;
        _handler = handler;
        
        Parameters.SetValue(dataSourceExpression.Parameters[0].Name!, dataSource);

        _propertyValueGenerator = dataSourceExpression.Compile();
        Update(true);
    }

    protected override void Update(bool couldCompute)
    {
        TExpressionResult? value = default;
        var parsed = false;

        try
        {
            if (couldCompute)
            {
                value = _propertyValueGenerator(_dataSource);
                parsed = true;
            }
        }
        catch
        {
        }

        _handler(parsed, value);
    }
}