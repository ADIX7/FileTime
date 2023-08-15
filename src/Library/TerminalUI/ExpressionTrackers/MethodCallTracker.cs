using System.Linq.Expressions;
using System.Reflection;

namespace TerminalUI.ExpressionTrackers;

public sealed class MethodCallTracker : ExpressionTrackerBase
{
    private readonly MethodInfo _method;
    private readonly IExpressionTracker? _objectTracker;
    private readonly List<IExpressionTracker> _argumentTrackers;

    public MethodCallTracker(
        MethodCallExpression methodCallExpression,
        IExpressionTracker? objectTracker,
        IEnumerable<IExpressionTracker> argumentTrackers)
    {
        _method = methodCallExpression.Method;
        _objectTracker = objectTracker;
        _argumentTrackers = argumentTrackers.ToList();

        if (objectTracker is not null)
        {
            SubscribeToTracker(objectTracker);
        }

        foreach (var expressionTracker in _argumentTrackers)
        {
            SubscribeToTracker(expressionTracker);
        }

        UpdateValueAndChangeTrackers();
    }

    protected override object? ComputeValue()
    {
        var obj = _objectTracker?.GetValue();
        if (obj is null && !_method.IsStatic) return null;
        return _method.Invoke(obj, _argumentTrackers.Select(t => t.GetValue()).ToArray());
    }
}