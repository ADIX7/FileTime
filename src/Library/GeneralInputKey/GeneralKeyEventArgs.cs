namespace GeneralInputKey;

public class GeneralKeyEventArgs
{
    private readonly Action<bool>? _handledChanged;
    private bool _handled;
    public required Keys Key { get; init; }
    public required char KeyChar { get; init; }
    public required SpecialKeysStatus SpecialKeysStatus { get; init; }

    public bool Handled
    {
        get => _handled;
        set
        {
            if (_handled != value)
            {
                _handled = value;
                _handledChanged?.Invoke(value);
            }
        }
    }

    public GeneralKeyEventArgs(Action<bool>? handledChanged = null)
    {
        _handledChanged = handledChanged;
    }
}