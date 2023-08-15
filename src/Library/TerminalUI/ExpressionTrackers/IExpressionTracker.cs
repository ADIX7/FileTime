namespace TerminalUI.ExpressionTrackers;

public interface IExpressionTracker
{
    List<string> TrackedPropertyNames { get; }
    event Action<string>? PropertyChanged;
    event Action<bool>? Update;
    object? GetValue();
}