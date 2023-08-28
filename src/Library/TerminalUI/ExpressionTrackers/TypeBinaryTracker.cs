using System.Linq.Expressions;

namespace TerminalUI.ExpressionTrackers;

public class TypeBinaryTracker : ExpressionTrackerBase
{
    private readonly IExpressionTracker _childExpressionTracker;
    private readonly Func<object?, bool> _computer;

    public TypeBinaryTracker(TypeBinaryExpression typeBinaryExpression, IExpressionTracker childExpressionTracker)
    {
        _childExpressionTracker = childExpressionTracker;
        ArgumentNullException.ThrowIfNull(childExpressionTracker);
        
        SubscribeToValueChanges = false;
        SubscribeToTracker(childExpressionTracker);
        
        _computer = typeBinaryExpression.NodeType switch
        {
            ExpressionType.TypeIs => o => o is not null && typeBinaryExpression.TypeOperand.IsInstanceOfType(o),
            _ => throw new NotImplementedException()
        };
    }
    protected override object? ComputeValue() 
        => _computer(_childExpressionTracker.GetValue());
}