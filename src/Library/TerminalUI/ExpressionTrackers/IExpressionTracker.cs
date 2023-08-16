namespace TerminalUI.ExpressionTrackers;

public interface IExpressionTracker
{
    event Action<string>? PropertyChanged;
    event Action<bool>? Update;
    object? GetValue();
    void TrackProperty(string propertyName);
}