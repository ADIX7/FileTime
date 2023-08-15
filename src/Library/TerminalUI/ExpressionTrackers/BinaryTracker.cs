using System.Linq.Expressions;

namespace TerminalUI.ExpressionTrackers;

public class BinaryTracker : ExpressionTrackerBase
{
    private readonly IExpressionTracker _leftExpressionTracker;
    private readonly IExpressionTracker _rightExpressionTracker;
    private readonly Func<object?, object?, object?> _computer;

    public BinaryTracker(
        BinaryExpression binaryExpression,
        IExpressionTracker leftExpressionTracker,
        IExpressionTracker rightExpressionTracker)
    {
        _leftExpressionTracker = leftExpressionTracker;
        _rightExpressionTracker = rightExpressionTracker;
        ArgumentNullException.ThrowIfNull(leftExpressionTracker);
        ArgumentNullException.ThrowIfNull(rightExpressionTracker);

        SubscribeToValueChanges = false;
        SubscribeToTracker(leftExpressionTracker);
        SubscribeToTracker(rightExpressionTracker);

        _computer = binaryExpression.NodeType switch
        {
            ExpressionType.Equal => (v1, v2) => Equals(v1, v2),
            ExpressionType.NotEqual => (v1, v2) => !Equals(v1, v2),
            _ => throw new NotImplementedException()
        };

        UpdateValueAndChangeTrackers();
    }

    protected override object? ComputeValue()
        => _computer(_leftExpressionTracker.GetValue(), _rightExpressionTracker.GetValue());
}