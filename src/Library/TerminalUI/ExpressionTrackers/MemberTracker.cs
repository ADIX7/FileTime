using System.Linq.Expressions;
using System.Reflection;

namespace TerminalUI.ExpressionTrackers;

public sealed class MemberTracker : ExpressionTrackerBase
{
    private readonly IExpressionTracker? _parentTracker;
    private readonly string _memberName;
    private readonly Func<object?> _valueProvider;

    public MemberTracker(MemberExpression memberExpression, IExpressionTracker? parentTracker)
    {
        ArgumentNullException.ThrowIfNull(memberExpression);
        _parentTracker = parentTracker;

        if (parentTracker is not null)
        {
            parentTracker.PropertyChanged += propertyName =>
            {
                if (propertyName == _memberName)
                {
                    UpdateValueAndChangeTrackers();
                }
            };
        }

        if (memberExpression.Member is PropertyInfo propertyInfo)
        {
            _memberName = propertyInfo.Name;
            parentTracker?.TrackProperty(propertyInfo.Name);

            if (propertyInfo.GetMethod is { } getMethod)
            {
                _valueProvider = () => CallPropertyInfo(propertyInfo);
            }
            else
            {
                throw new NotSupportedException(
                    $"Try to get value of a property without a getter: {propertyInfo.Name} in {propertyInfo.DeclaringType?.Name ?? "<null>"}."
                );
            }
        }
        else if (memberExpression.Member is FieldInfo fieldInfo)
        {
            _memberName = fieldInfo.Name;
            parentTracker?.TrackProperty(fieldInfo.Name);

            _valueProvider = () => CallFieldInfo(fieldInfo);
        }
        else
        {
            throw new NotSupportedException($"Could not determine source type of expression {memberExpression} with parent {parentTracker}");
        }

        SubscribeToTracker(parentTracker);
        UpdateValueAndChangeTrackers();
    }

    private object? CallPropertyInfo(PropertyInfo propertyInfo)
    {
        var obj = _parentTracker?.GetValue();
        if (obj is null && !propertyInfo.GetMethod!.IsStatic) return null;

        return propertyInfo.GetValue(_parentTracker?.GetValue());
    }

    private object? CallFieldInfo(FieldInfo fieldInfo)
    {
        var obj = _parentTracker?.GetValue();
        if (obj is null && !fieldInfo.IsStatic) return null;

        return fieldInfo.GetValue(_parentTracker?.GetValue());
    }

    protected override object? ComputeValue()
    {
        var v = _valueProvider();

        return v;
    }
}