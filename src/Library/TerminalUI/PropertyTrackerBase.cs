using System.Linq.Expressions;
using TerminalUI.ExpressionTrackers;

namespace TerminalUI;

public abstract class PropertyTrackerBase<TSource, TExpressionResult>
{
    private readonly IExpressionTracker _tracker;
    protected ExpressionParameterTrackerCollection Parameters { get; } = new();

    protected PropertyTrackerBase(Expression<Func<TSource?, TExpressionResult>> dataSourceExpression)
    {
        ArgumentNullException.ThrowIfNull(dataSourceExpression);

        _tracker = FindReactiveProperties(dataSourceExpression.Body, Parameters);
        _tracker.Update += Update;
    }

    private IExpressionTracker FindReactiveProperties(Expression? expression, ExpressionParameterTrackerCollection parameters)
    {
        if (expression is ConditionalExpression conditionalExpression)
        {
            var testTracker = FindReactiveProperties(conditionalExpression.Test, parameters);
            var trueTracker = FindReactiveProperties(conditionalExpression.IfTrue, parameters);
            var falseTracker = FindReactiveProperties(conditionalExpression.IfFalse, parameters);

            return new ConditionalTracker(
                conditionalExpression,
                testTracker,
                trueTracker,
                falseTracker);
        }
        else if (expression is MemberExpression memberExpression)
        {
            IExpressionTracker? parentExpressionTracker = null;
            if (memberExpression.Expression is not null)
            {
                parentExpressionTracker = FindReactiveProperties(memberExpression.Expression, parameters);
            }

            return new MemberTracker(memberExpression, parentExpressionTracker);
        }
        else if (expression is MethodCallExpression methodCallExpression)
        {
            IExpressionTracker? objectTracker = null;
            if (methodCallExpression.Object is { } methodObject)
            {
                objectTracker = FindReactiveProperties(methodObject, parameters);
            }

            var argumentTrackers = new List<IExpressionTracker>(methodCallExpression.Arguments.Count);
            foreach (var argument in methodCallExpression.Arguments)
            {
                var argumentTracker = FindReactiveProperties(argument, parameters);
                argumentTrackers.Add(argumentTracker);
            }

            return new MethodCallTracker(methodCallExpression, objectTracker, argumentTrackers);
        }
        else if (expression is BinaryExpression binaryExpression)
        {
            var leftTracker = FindReactiveProperties(binaryExpression.Left, parameters);
            var rightTracker = FindReactiveProperties(binaryExpression.Right, parameters);

            return new BinaryTracker(binaryExpression, leftTracker, rightTracker);
        }
        else if (expression is UnaryExpression unaryExpression)
        {
            var operandTracker = FindReactiveProperties(unaryExpression.Operand, parameters);
            return new UnaryTracker(unaryExpression, operandTracker);
        }
        else if (expression is ParameterExpression parameterExpression)
        {
            if (parameterExpression.Name is { } name)
            {
                return new ParameterTracker(parameterExpression, parameters, name);
            }
        }
        else if (expression is ConstantExpression constantExpression)
        {
            return new ConstantTracker(constantExpression.Value);
        }
        /*else if (expression is not ConstantExpression)
        {
            Debug.Assert(false, "Unknown expression type " + expression.GetType());
        }*/

        throw new NotSupportedException();
    }

    protected abstract void Update(bool couldCompute);
}