using System.Linq.Expressions;

namespace TerminalUI.ExpressionTrackers;

public sealed class ParameterTracker : ExpressionTrackerBase
{
    private readonly ExpressionParameterTrackerCollection _trackerCollection;
    private readonly string _parameterName;

    public ParameterTracker(
        ParameterExpression parameterExpression, 
        ExpressionParameterTrackerCollection trackerCollection,
        string parameterName)
    {
        _trackerCollection = trackerCollection;
        _parameterName = parameterName;
        
        trackerCollection.ValueChanged += TrackerCollectionOnValueChanged;
        
        UpdateValueAndChangeTrackers();
    }

    private void TrackerCollectionOnValueChanged(string parameterName, object? newValue) 
        => UpdateValueAndChangeTrackers();

    protected override object? ComputeValue()
    {
        _trackerCollection.Values.TryGetValue(_parameterName, out var v);
        return v;
    }
}