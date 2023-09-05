using System.Linq.Expressions;

namespace TerminalUI.ExpressionTrackers;

public class ConditionalTracker : ExpressionTrackerBase
{
    private readonly IExpressionTracker _testExpressionTracker;
    private readonly IExpressionTracker _ifTrueExpressionTracker;
    private readonly IExpressionTracker _ifFalseExpressionTracker;

    public ConditionalTracker(
        ConditionalExpression conditionalExpression,
        IExpressionTracker testExpressionTracker,
        IExpressionTracker ifTrueExpressionTracker,
        IExpressionTracker ifFalseExpressionTracker)
    {
        ArgumentNullException.ThrowIfNull(conditionalExpression);
        ArgumentNullException.ThrowIfNull(testExpressionTracker);
        ArgumentNullException.ThrowIfNull(ifTrueExpressionTracker);
        
        SubscribeToValueChanges = false;

        _testExpressionTracker = testExpressionTracker;
        _ifTrueExpressionTracker = ifTrueExpressionTracker;
        _ifFalseExpressionTracker = ifFalseExpressionTracker;
        
        SubscribeToTracker(testExpressionTracker);
        SubscribeToTracker(ifTrueExpressionTracker);
        SubscribeToTracker(ifFalseExpressionTracker);
        
        UpdateValueAndChangeTrackers();
    }

    protected override object? ComputeValue()
    {
        var testValue = _testExpressionTracker.GetValue();
        return testValue switch
        {
            true => _ifTrueExpressionTracker.GetValue(),
            false => _ifFalseExpressionTracker.GetValue(),
            _ => throw new NotSupportedException($"Conditional expression must evaluate to a boolean value, but {testValue} ({testValue?.GetType().Name}) is not that.")
        };
    }
}