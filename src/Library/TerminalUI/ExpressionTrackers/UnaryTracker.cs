using System.Linq.Expressions;

namespace TerminalUI.ExpressionTrackers;

public class UnaryTracker : ExpressionTrackerBase
{
    private readonly IExpressionTracker _operandTracker;
    private readonly Func<object?, object?> _operator;

    public UnaryTracker(UnaryExpression unaryExpression, IExpressionTracker operandTracker)
    {
        _operandTracker = operandTracker;
        ArgumentNullException.ThrowIfNull(unaryExpression);
        ArgumentNullException.ThrowIfNull(operandTracker);

        SubscribeToValueChanges = false;

        _operator = unaryExpression.NodeType switch
        {
            ExpressionType.Negate => Negate,
            ExpressionType.Not => o => o is bool b ? !b : null,
            ExpressionType.Convert => o => o,
            _ => throw new NotSupportedException($"Unary expression of type {unaryExpression.NodeType} is not supported.")
        };

        SubscribeToTracker(operandTracker);

        UpdateValueAndChangeTrackers();
    }

    private static object? Negate(object? source)
    {
        if (source is null) return null;
        return source switch
        {
            int i => -i,
            long l => -l,
            float f => -f,
            double d => -d,
            decimal d => -d,
            _ => throw new NotSupportedException($"Unary negation is not supported for type {source.GetType().Name}.")
        };
    }

    protected override object? ComputeValue() => _operator(_operandTracker.GetValue());
}