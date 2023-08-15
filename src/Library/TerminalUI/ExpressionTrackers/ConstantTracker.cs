namespace TerminalUI.ExpressionTrackers;

public class ConstantTracker : ExpressionTrackerBase
{
    private readonly object? _value;

    public ConstantTracker(object? value)
    {
        _value = value;
        UpdateValueAndChangeTrackers();
    }
    protected override object? ComputeValue() => _value;
}